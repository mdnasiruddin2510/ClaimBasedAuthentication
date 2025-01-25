using ClaimBasedAuthentication.Application.IRepository;
using ClaimBasedAuthentication.Application.ViewModel;
using ClaimBasedAuthentication.Domain.Entities;
using ClaimBasedAuthentication.Domain.Helper;
using ClaimBasedAuthentication.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Persistence.Repositories
{
    public class PersonalInfoRepository : IPersonalInfoRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;
        public PersonalInfoRepository(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;  
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<VmResponseMessage> CreatePersonalInfo(VmPersonalInfo vm)
        {
            var response = new VmResponseMessage();
            var photoUrl = "";
            if (vm.Photo is not null) photoUrl = AppFunction.FileUpload(_webHostEnvironment, vm.Photo, "PersonalInfo");
            var existPersonalInfo = await _db.PersonalInfo
                                             .FirstOrDefaultAsync(x => x.Email.Equals(vm.Email)
                                             && x.Phone.Equals(vm.Phone));
            if (existPersonalInfo is null)
            {
                var model = new PersonalInfo
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    Address = vm.Address,
                    Gender = vm.Gender,
                    BloodGroup = vm.BloodGroup,
                    DOB = Convert.ToDateTime(vm.DOB),
                    PhotoUrl = photoUrl
                };
                await _db.PersonalInfo.AddAsync(model);
                await _db.SaveChangesAsync();
                response.Type = "Success";
                response.Message = "Successfully Saved!";
            }
            else 
            {
                response.Type = "Error";
                response.Message = "Already Exist!";
            }
            return response;
        }
    }
}
