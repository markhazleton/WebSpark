using PromptSpark.Domain.Service;


namespace WebSpark.Portal.Areas.PromptSpark.Controllers.Api;

[Route("api/[area]/[controller]")]
public class VariantController(IGPTDefinitionService definitionService) : BasePromptSparkApiController
{
    /// <summary>
    /// Returns a list of all variants
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Get()
    {
        try
        {
            var list = await definitionService.GetDefinitionsAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }
}
