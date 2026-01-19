using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Schedora.Application;
using Schedora.Application.Requests;
using Schedora.Domain.DomainServices;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using Schedora.UnitTests.Mocks;

namespace Schedora.UnitTests.Services;

public class MediaTests
{
    private readonly Mock<IStorageService>  _storageService;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<IMediaDomainService> _mediaDomainService;
    private readonly Mock<IMediaHandlerService> _mediaHandlerService;

    public MediaTests()
    {
        _storageService = new Mock<IStorageService>();
        _mapper = new Mock<IMapper>();
        _uow = new UnitOfWorkMock().GetMock();
        _currentUser = new Mock<ICurrentUserService>();
        _mediaDomainService = new Mock<IMediaDomainService>();
        _mediaHandlerService = new Mock<IMediaHandlerService>();
    }

    [Fact]
    public async Task InvalidMediaTypeShould_ThrowException()
    {
        var content = "fake image content";
        var fileName = "test.png";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        var request = new UploadMediaRequest()
        {
            Description = "best image",
            FolderId = null,
            
        }
    }
}