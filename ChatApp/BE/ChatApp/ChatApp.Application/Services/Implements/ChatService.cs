using AutoMapper;
using ChatApp.Application.Services.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Services.Implements
{
    public class ChatService : BaseService<ChatService>, IChatService
    {
        public ChatService(IUnitOfWork<ChatAppDbContext> unitOfWork, ILogger<ChatService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) 
            : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }
    }
}
