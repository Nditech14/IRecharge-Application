using Application.DTO.RequestDto;
using Application.DTO.ResponseDto;

namespace Application.Service.Abstraction
{
    public interface IBillService
    {
        Task<BillResponseDto> CreateBillAsync(CreateBillRequest request);
        Task<BillResponseDto> PayBillAsync(string id);
    }
}