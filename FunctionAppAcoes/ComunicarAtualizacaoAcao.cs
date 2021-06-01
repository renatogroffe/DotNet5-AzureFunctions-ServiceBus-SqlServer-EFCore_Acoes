using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using FunctionAppAcoes.Models;
using FunctionAppAcoes.Validators;

namespace FunctionAppAcoes
{
    public static class ComunicarAtualizacaoAcao
    {
        private const string MSG_ERRO_SERIALIZACAO = "Erro durante a deserializacao dos dados recebidos!";
        private const string MSG_ERRO_VALIDACAO = "Dados invalidos para a Acao";
        private const string MSG_SUCESSO_NOTIFICACAO = "Notificação de atualização de ação realizada com sucesso!";

        private static HttpResponseData CreateResponseBadRequest(
            HttpRequestData req, string mensagem)
        {
            var responseBadRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            responseBadRequest.WriteAsJsonAsync(
                new
                {
                    Sucesso = false,
                    Mensagem = mensagem
                });
            return responseBadRequest;
        }


        [Function("ComunicarAtualizacaoAcao")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ComunicarAtualizacaoAcao");

            var conteudoDadosAcao = req.ReadAsString();
            logger.LogInformation($"Dados recebidos: {conteudoDadosAcao}");

            DadosAcao dadosAcao = null;
            try
            {
                dadosAcao = JsonSerializer.Deserialize<DadosAcao>(conteudoDadosAcao,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                logger.LogError(MSG_ERRO_SERIALIZACAO);
            }

            if (dadosAcao == null)
                return CreateResponseBadRequest(req, MSG_ERRO_SERIALIZACAO);

            var validationResult = new DadosAcaoValidator().Validate(dadosAcao);
            if (validationResult.IsValid)
            {
                var topic = Environment.GetEnvironmentVariable("AzureServiceBus_Topic");
                var client = new TopicClient(
                    Environment.GetEnvironmentVariable("AzureServiceBus_Connection"),
                    topic);
                client.SendAsync(
                    new Message(Encoding.UTF8.GetBytes(conteudoDadosAcao))).Wait();
                logger.LogInformation(
                    $"Azure Service Bus - Envio para o tópico {conteudoDadosAcao} concluído");

                logger.LogInformation(MSG_SUCESSO_NOTIFICACAO);

                var responseOKRequest = req.CreateResponse(HttpStatusCode.OK);
                responseOKRequest.WriteAsJsonAsync(
                    new
                    {
                        Sucesso = true,
                        Mensagem = MSG_SUCESSO_NOTIFICACAO
                    });
                return responseOKRequest;
            }
            else
            {
                logger.LogError(MSG_ERRO_VALIDACAO);
                foreach (var error in validationResult.Errors)
                    logger.LogError($" ## {error.ErrorMessage}");

                return CreateResponseBadRequest(req, $"{MSG_ERRO_VALIDACAO}: "+
                    String.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }
        }
    }
}