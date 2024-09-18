using Microsoft.EntityFrameworkCore;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service
{
    public class ResponseService(GPTDbContext context) : IResponseService
    {
        public async Task<List<DefinitionResponseDto>> GetAllResponsesAsync()
        {
            var list = await context.DefinitionResponses.ToListAsync() ?? [];
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<DefinitionResponseDto> GetResponseByIdAsync(int? id)
        {
            var item = await context.DefinitionResponses
                .FirstOrDefaultAsync(m => m.Id == id) ?? new GPTDefinitionResponse();
            return item.ToDto();
        }

        public async Task AddResponseAsync(DefinitionResponseDto gPTResponse)
        {
            context.Add(gPTResponse);
            await context.SaveChangesAsync();
        }

        public async Task UpdateResponseAsync(DefinitionResponseDto gPTResponse)
        {
            context.Update(gPTResponse);
            await context.SaveChangesAsync();
        }

        public async Task DeleteResponseAsync(int id)
        {
            var gPTResponse = await context.Chats.FindAsync(id);
            if (gPTResponse != null)
            {
                context.Chats.Remove(gPTResponse);
                await context.SaveChangesAsync();
            }
        }

        public bool ResponseExists(int id)
        {
            return context.Chats.Any(e => e.Id == id);
        }
    }
}
