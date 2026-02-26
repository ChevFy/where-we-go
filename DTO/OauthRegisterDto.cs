
using System.ComponentModel.DataAnnotations;

namespace where_we_go.DTO
{
    public class OauthRegisterDto
    {
        public string Email {get ; set;} = string.Empty; 
        
        public string Name {get; set;} = string.Empty;

        public  string UserName {get; set;} = string.Empty;


        public string? ConfirmPassword {get; set;} = string.Empty;

        public  string LoginProvider {get; set;} = string.Empty;

        public  string ProviderKey {get; set;} = string.Empty;


    }
    
}