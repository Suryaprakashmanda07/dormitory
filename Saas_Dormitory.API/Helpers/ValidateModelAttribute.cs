using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Saas_Dormitory.Models.ResponseDTO;

namespace Saas_Dormitory.API.Helpers
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var response = new ResponseDTO();
                response.Valid = false;
                response.Msg = string.Join("; ", context.ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                context.Result = new BadRequestObjectResult(response); // it returns 400 with the error
            }
        }
    }
}
