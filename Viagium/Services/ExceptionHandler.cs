using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Viagium.Services
{
    public class ExceptionHandler
    {
        /// <summary>
        /// Trata exceções específicas e retorna uma resposta HTTP apropriada
        /// </summary>
        /// <param name="ex">Exceção a ser tratada</param>
        /// <returns>IActionResult com a resposta HTTP apropriada</returns>
        public static IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                ArgumentException argEx => new BadRequestObjectResult(new 
                { 
                    error = "Dados inválidos", 
                    message = argEx.Message,
                    timestamp = DateTime.UtcNow
                }),
                
                ValidationException valEx => new BadRequestObjectResult(new 
                { 
                    error = "Erro de validação", 
                    message = valEx.Message,
                    timestamp = DateTime.UtcNow
                }),
                
                InvalidOperationException invEx => new ConflictObjectResult(new 
                { 
                    error = "Operação inválida", 
                    message = invEx.Message,
                    timestamp = DateTime.UtcNow
                }),
                
                DbUpdateException dbEx => new ObjectResult(new 
                { 
                    error = "Erro de banco de dados", 
                    message = "Erro ao salvar os dados. Verifique se os dados são válidos.",
                    timestamp = DateTime.UtcNow
                })
                { StatusCode = 500 },
                
                UnauthorizedAccessException => new UnauthorizedObjectResult(new 
                { 
                    error = "Acesso negado", 
                    message = "Você não tem permissão para realizar esta operação.",
                    timestamp = DateTime.UtcNow
                }),
                
                _ => new ObjectResult(new 
                { 
                    error = "Erro interno do servidor", 
                    message = "Ocorreu um erro inesperado. Tente novamente mais tarde.",
                    timestamp = DateTime.UtcNow
                })
                { StatusCode = 500 }
            };
        }

        /// <summary>
        /// Trata exceções relacionadas ao UnitOfWork e banco de dados
        /// </summary>
        /// <param name="ex">Exceção original</param>
        /// <param name="operation">Nome da operação que falhou</param>
        /// <returns>InvalidOperationException com mensagem apropriada</returns>
        public static InvalidOperationException HandleDatabaseException(Exception ex, string operation = "operação de banco")
        {
            return ex switch
            {
                DbUpdateConcurrencyException => new InvalidOperationException(
                    "Conflito de concorrência: os dados foram modificados por outro usuário.", ex),
                
                DbUpdateException when ex.InnerException?.Message.Contains("UNIQUE") == true => 
                    new InvalidOperationException("Violação de unicidade: já existe um registro com estes dados.", ex),
                
                DbUpdateException when ex.InnerException?.Message.Contains("FOREIGN KEY") == true => 
                    new InvalidOperationException("Referência inválida: verifique se os dados relacionados existem.", ex),
                
                TimeoutException => new InvalidOperationException(
                    "Timeout: a operação demorou muito para ser concluída.", ex),
                
                _ => new InvalidOperationException($"Erro ao executar {operation}: {ex.Message}", ex)
            };
        }

        /// <summary>
        /// Valida um objeto usando DataAnnotations
        /// </summary>
        /// <param name="obj">Objeto a ser validado</param>
        /// <param name="objectName">Nome do objeto para mensagens de erro</param>
        /// <exception cref="ArgumentException">Lançada quando a validação falha</exception>
        public static void ValidateObject(object obj, string objectName = "objeto")
        {
            if (obj == null)
                throw new ArgumentException($"O {objectName} não pode ser nulo.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(obj);

            if (!Validator.TryValidateObject(obj, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                throw new ArgumentException($"Erro de validação no {objectName}:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Executa uma operação com tratamento automático de exceções
        /// </summary>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="operationName">Nome da operação para logs</param>
        /// <returns>Resultado da operação</returns>
        public static async Task<T> ExecuteWithHandling<T>(Func<Task<T>> operation, string operationName = "operação")
        {
            try
            {
                return await operation();
            }
            catch (DbUpdateException dbEx)
            {
                throw HandleDatabaseException(dbEx, operationName);
            }
            catch (TimeoutException timeEx)
            {
                throw HandleDatabaseException(timeEx, operationName);
            }
            catch (ValidationException)
            {
                throw; // Re-lança exceções de validação sem modificar
            }
            catch (ArgumentException)
            {
                throw; // Re-lança exceções de argumento sem modificar
            }
            catch (Exception ex)
            {
                throw HandleDatabaseException(ex, operationName);
            }
        }

        /// <summary>
        /// Executa uma operação com tratamento automático de exceções (versão síncrona)
        /// </summary>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="operationName">Nome da operação para logs</param>
        /// <returns>Resultado da operação</returns>
        public static T ExecuteWithHandling<T>(Func<T> operation, string operationName = "operação")
        {
            try
            {
                return operation();
            }
            catch (DbUpdateException dbEx)
            {
                throw HandleDatabaseException(dbEx, operationName);
            }
            catch (TimeoutException timeEx)
            {
                throw HandleDatabaseException(timeEx, operationName);
            }
            catch (ValidationException)
            {
                throw; // Re-lança exceções de validação sem modificar
            }
            catch (ArgumentException)
            {
                throw; // Re-lança exceções de argumento sem modificar
            }
            catch (Exception ex)
            {
                throw HandleDatabaseException(ex, operationName);
            }
        }
    }
}
