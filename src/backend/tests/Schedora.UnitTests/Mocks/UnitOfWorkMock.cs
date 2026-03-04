using Moq;
using Schedora.Domain.Interfaces;

namespace Schedora.UnitTests.Mocks;

public class UnitOfWorkMock
{
    private readonly Mock<IUnitOfWork>  _unitOfWork =  new Mock<IUnitOfWork>();
    public Mock<IGenericRepository> GenericRepository { get; set; } = new Mock<IGenericRepository>();
    public Mock<IUserRepository> UserRepository { get; set; } = new Mock<IUserRepository>();
    public Mock<ISocialAccountRepository> SocialAccountRepository { get; set; } = new Mock<ISocialAccountRepository>();
    public Mock<IPostRepository> PostRepository { get; set; } = new Mock<IPostRepository>();

    public UnitOfWorkMock()
    {
        _unitOfWork.SetupGet(d => d.GenericRepository).Returns(GenericRepository.Object);
        _unitOfWork.SetupGet(d => d.UserRepository).Returns(UserRepository.Object);
        _unitOfWork.SetupGet(d => d.SocialAccountRepository).Returns(SocialAccountRepository.Object);
        _unitOfWork.SetupGet(d => d.PostRepository).Returns(PostRepository.Object);
    }
    
    public Mock<IUnitOfWork> GetMock() => _unitOfWork;
    
    public IUnitOfWork GetObject() => _unitOfWork.Object;
}