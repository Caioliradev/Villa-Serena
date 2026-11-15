using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;

namespace Delivery
{
    public static class ImportadorPedidos
    {
        private class ArquivoJson
        {
            [JsonPropertyName("pedidos")] public List<PedidoDto> Pedidos { get; set; } = new List<PedidoDto>();
        }

        private class PedidoDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = "";
            [JsonPropertyName("data")] public DateTime Data { get; set; }
            [JsonPropertyName("cliente")] public ClienteDto Cliente { get; set; } = new ClienteDto();
            [JsonPropertyName("pagamento")] public string Pagamento { get; set; } = "";
            [JsonPropertyName("itens")] public List<ItemDto> Itens { get; set; } = new List<ItemDto>();
            [JsonPropertyName("total")] public decimal Total { get; set; }
        }

        private class ClienteDto
        {
            [JsonPropertyName("nome")] public string Nome { get; set; } = "";
            [JsonPropertyName("telefone")] public string Telefone { get; set; } = "";
            [JsonPropertyName("endereco")] public string Endereco { get; set; } = "";
        }

        private class ItemDto
        {
            [JsonPropertyName("codigo")] public string Codigo { get; set; } = "";
            [JsonPropertyName("quantidade")] public int Quantidade { get; set; }
            [JsonPropertyName("preco")] public decimal Preco { get; set; }
        }

        private static readonly string[] FormasValidas = { "pix", "cartao_credito", "cartao_debito", "dinheiro" };

        public static (int importados, int ignorados) Importar(string caminho)
        {
            return ImportarTexto(File.ReadAllText(caminho));
        }

        public static (int importados, int ignorados) ImportarTexto(string texto)
        {
            var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var arquivo = JsonSerializer.Deserialize<ArquivoJson>(texto, opcoes);

            if (arquivo == null || arquivo.Pedidos.Count == 0)
                throw new InvalidOperationException("O arquivo não contém pedidos.");

            int importados = 0, ignorados = 0;

            using (var conn = Conexao.ObterConexao())
            {
                conn.Open();

                foreach (var p in arquivo.Pedidos)
                {
                    using (var tx = conn.BeginTransaction())
                    {
                        MySqlCommand Cmd(string sql) => new MySqlCommand(sql, conn, tx);

                        var cmdExiste = Cmd("SELECT COUNT(*) FROM pedidos WHERE id_externo = @id");
                        cmdExiste.Parameters.AddWithValue("@id", p.Id);
                        if (Convert.ToInt32(cmdExiste.ExecuteScalar()) > 0)
                        {
                            ignorados++;
                            continue; 
                        }

                        string forma = (p.Pagamento ?? "").Trim().ToLowerInvariant();
                        if (Array.IndexOf(FormasValidas, forma) < 0)
                            throw new InvalidOperationException($"Pedido {p.Id}: forma de pagamento inválida ('{p.Pagamento}').");

                        var cmdCliente = Cmd("SELECT id FROM clientes WHERE nome = @n AND telefone = @t LIMIT 1");
                        cmdCliente.Parameters.AddWithValue("@n", p.Cliente.Nome);
                        cmdCliente.Parameters.AddWithValue("@t", p.Cliente.Telefone);
                        object achado = cmdCliente.ExecuteScalar();
                        long clienteId;
                        if (achado != null && achado != DBNull.Value)
                        {
                            clienteId = Convert.ToInt64(achado);
                        }
                        else
                        {
                            var cmdNovoCliente = Cmd("INSERT INTO clientes (nome, telefone, endereco) VALUES (@n, @t, @e)");
                            cmdNovoCliente.Parameters.AddWithValue("@n", p.Cliente.Nome);
                            cmdNovoCliente.Parameters.AddWithValue("@t", p.Cliente.Telefone);
                            cmdNovoCliente.Parameters.AddWithValue("@e", p.Cliente.Endereco);
                            cmdNovoCliente.ExecuteNonQuery();
                            clienteId = cmdNovoCliente.LastInsertedId;
                        }

                        var cmdPedido = Cmd(
                            @"INSERT INTO pedidos (id_externo, cliente_id, nome_cliente, telefone, endereco_entrega, status, total, criado_em)
                              VALUES (@id, @c, @nome, @tel, @end, 'pendente', @total, @data)");
                        cmdPedido.Parameters.AddWithValue("@id", p.Id);
                        cmdPedido.Parameters.AddWithValue("@c", clienteId);
                        cmdPedido.Parameters.AddWithValue("@nome", p.Cliente.Nome);
                        cmdPedido.Parameters.AddWithValue("@tel", p.Cliente.Telefone);
                        cmdPedido.Parameters.AddWithValue("@end", p.Cliente.Endereco);
                        cmdPedido.Parameters.AddWithValue("@total", p.Total);
                        cmdPedido.Parameters.AddWithValue("@data", p.Data);
                        cmdPedido.ExecuteNonQuery();
                        long pedidoId = cmdPedido.LastInsertedId;

                        foreach (var item in p.Itens)
                        {
                            var cmdProduto = Cmd("SELECT id FROM produtos WHERE codigo = @cod");
                            cmdProduto.Parameters.AddWithValue("@cod", item.Codigo);
                            object produtoId = cmdProduto.ExecuteScalar();
                            if (produtoId == null || produtoId == DBNull.Value)
                                throw new InvalidOperationException(
                                    $"Pedido {p.Id}: produto '{item.Codigo}' não está cadastrado na tabela produtos.");

                            var cmdItem = Cmd(
                                @"INSERT INTO itens_pedido (pedido_id, produto_id, quantidade, preco_unitario)
                                  VALUES (@pe, @pr, @q, @preco)
                                  ON DUPLICATE KEY UPDATE quantidade = quantidade + VALUES(quantidade)");
                            cmdItem.Parameters.AddWithValue("@pe", pedidoId);
                            cmdItem.Parameters.AddWithValue("@pr", Convert.ToInt64(produtoId));
                            cmdItem.Parameters.AddWithValue("@q", item.Quantidade);
                            cmdItem.Parameters.AddWithValue("@preco", item.Preco);
                            cmdItem.ExecuteNonQuery();
                        }

                        var cmdPag = Cmd("INSERT INTO pagamentos (pedido_id, forma, valor, status) VALUES (@pe, @f, @v, 'pendente')");
                        cmdPag.Parameters.AddWithValue("@pe", pedidoId);
                        cmdPag.Parameters.AddWithValue("@f", forma);
                        cmdPag.Parameters.AddWithValue("@v", p.Total);
                        cmdPag.ExecuteNonQuery();

                        tx.Commit();
                        importados++;
                    }
                }
            }
            return (importados, ignorados);
        }
    }
}
