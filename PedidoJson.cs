using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Delivery
{
    public class ArquivoPedidos
    {
        [JsonPropertyName("pedidos")]
        public List<PedidoJson> Pedidos { get; set; }
    }

    public class PedidoJson
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("data")] public DateTime Data { get; set; }
        [JsonPropertyName("cliente")] public ClienteJson Cliente { get; set; }
        [JsonPropertyName("pagamento")] public string Pagamento { get; set; }
        [JsonPropertyName("itens")] public List<ItemJson> Itens { get; set; }
        [JsonPropertyName("total")] public decimal Total { get; set; }
    }

    public class ClienteJson
    {
        [JsonPropertyName("nome")] public string Nome { get; set; }
        [JsonPropertyName("telefone")] public string Telefone { get; set; }
        [JsonPropertyName("endereco")] public string Endereco { get; set; }
    }

    public class ItemJson
    {
        [JsonPropertyName("codigo")] public string Codigo { get; set; }
        [JsonPropertyName("quantidade")] public int Quantidade { get; set; }
        [JsonPropertyName("preco")] public decimal Preco { get; set; }
    }
}