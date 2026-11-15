using System.Net;
using System.Text;

namespace Delivery
{
    // Recebe o JSON do site em http://localhost:5000/pedido/ e grava no MySQL.
    public static class ServidorPedidos
    {
        public static void Iniciar()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/pedido/");
            try { listener.Start(); }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível abrir a porta 5000 para receber pedidos do site:\n" + ex.Message);
                return;
            }

            Task.Run(() =>
            {
                while (listener.IsListening)
                {
                    HttpListenerContext ctx;
                    try { ctx = listener.GetContext(); } catch { break; }
                    var res = ctx.Response;

                    // CORS: o site abre em outro endereço/porta
                    res.AddHeader("Access-Control-Allow-Origin", "*");
                    res.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                    res.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");

                    if (ctx.Request.HttpMethod == "OPTIONS")
                    {
                        res.StatusCode = 204;
                        res.Close();
                        continue;
                    }

                    string resposta;
                    try
                    {
                        using var sr = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
                        var (importados, ignorados) = ImportadorPedidos.ImportarTexto(sr.ReadToEnd());
                        res.StatusCode = 200;
                        resposta = $"{{\"importados\":{importados},\"ignorados\":{ignorados}}}";
                    }
                    catch (Exception ex)
                    {
                        res.StatusCode = 500;
                        resposta = ex.Message;
                    }

                    var bytes = Encoding.UTF8.GetBytes(resposta);
                    res.ContentType = "application/json; charset=utf-8";
                    res.OutputStream.Write(bytes, 0, bytes.Length);
                    res.Close();
                }
            });
        }
    }
}
