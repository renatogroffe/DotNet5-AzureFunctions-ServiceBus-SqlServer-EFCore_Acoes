using System.Linq;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FunctionAppAcoes.Data;

namespace FunctionAppAcoes
{
    public class AcoesEF
    {
        private readonly AcoesContext _context;

        public AcoesEF(AcoesContext context)
        {
            _context = context;
        }

        [Function("AcoesEF")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AcoesEF");

            var historicoAcoes = _context.Acoes.OrderByDescending(a => a.DataReferencia);
            logger.LogInformation($"No. de documentos encontrados: {historicoAcoes.Count()}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteAsJsonAsync(historicoAcoes);
            return response;
        }
    }
}
