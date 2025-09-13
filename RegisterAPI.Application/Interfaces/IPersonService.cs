using RegisterAPI.Application.DTOs;
using System.Collections.Generic;

namespace RegisterAPI.Application.Interfaces
{
    public interface IPersonService
    {
        PersonResponseDto Create(PersonDto dto);
        PersonResponseDto Update(int id, PersonDto dto);
        void Delete(int id);
        PersonResponseDto? GetById(int id);
        IEnumerable<PersonResponseDto> GetAll();
    }

    public interface IPersonServiceV2
    {
        PersonV2ResponseDto Create(PersonV2Dto dto);
        PersonV2ResponseDto Update(int id, PersonV2Dto dto);
        void Delete(int id);
        PersonV2ResponseDto? GetById(int id);
        IEnumerable<PersonV2ResponseDto> GetAll();
    }
}
