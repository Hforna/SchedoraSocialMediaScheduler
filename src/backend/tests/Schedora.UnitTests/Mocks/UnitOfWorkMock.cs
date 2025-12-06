using Moq;
using Schedora.Domain.Interfaces;

namespace Schedora.UnitTests.Mocks;

public class UnitOfWorkMock
{
    private readonly Mock<IUnitOfWork>  _unitOfWork =  new Mock<IUnitOfWork>();
    public Mock<IGenericRepository> GenericRepository { get; set; } = new Mock<IGenericRepository>();
    public Mock<IUserRepository> UserRepository { get; set; } = new Mock<IUserRepository>();

    public UnitOfWorkMock()
    {
        _unitOfWork.SetupGet(d => d.GenericRepository).Returns(GenericRepository.Object);
        _unitOfWork.SetupGet(d => d.UserRepository).Returns(UserRepository.Object);
    }
    
    public Mock<IUnitOfWork> GetMock() => _unitOfWork;
    
    public IUnitOfWork GetObject() => _unitOfWork.Object;
}