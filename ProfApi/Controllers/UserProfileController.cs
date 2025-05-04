using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfApi.DBcontext;
using ProfApi.Models;
using ProfApi.Models.ScrollDTO;
using ProfApi.Models.UserDTO;
using ProfApi.Services;
using System.Security.Claims;

namespace ProfApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : Controller
    {

        private readonly ProfDbContext _context;
        private readonly ILogger<UserProfileController> _logger;
        private readonly FileFolderService _fileFolderService;
        private string [] _allowedExtensions = { ".jpg", ".jpeg", ".png" };


        public UserProfileController(ProfDbContext context, ILogger<UserProfileController> logger, FileFolderService fileFolderService)
        {
            _context = context;
            _logger = logger;
            _fileFolderService = fileFolderService;
        }

        [Authorize]
        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers([FromQuery] int lastUserId = 0, [FromQuery] string name = "")
        {
            int pageSize = 10;

            var userList = _context.Users
                .Where(user => (lastUserId == 0 || user.UserId > lastUserId) &&
                               (string.IsNullOrEmpty(name) || user.UserName.Contains(name))) 
                .Select(user => new UserListDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture,   
                  
                });

            var totalRecords = await userList.CountAsync();  

            var users = await userList
                .Take(pageSize)  
                .ToListAsync();

            bool hasMore = users.Count == pageSize;  

            int lastId = users.Any() ? users.Last().UserId : 0;  

            var result = new ScrollDTO<UserListDTO>
            {
                Data = users,
                TotalRecords = totalRecords,
                LastId = lastId,
                HasMore = hasMore
            };

            _logger.LogInformation("Listado paginado de usuarios");

            return Ok(result);  
        }

        [Authorize]
        [HttpGet("Workshop")]
        public async Task<IActionResult> GetWorkshop([FromQuery] int lastUserId = 0, [FromQuery] string name = "")
        {
            int pageSize = 10;

            var shopList = _context.Users
                .Where(user => (lastUserId == 0 || user.UserId > lastUserId) &&
                               (string.IsNullOrEmpty(name) || user.UserName.Contains(name)) &&
                               user.Type == UserType.Workshop)  
                .Select(user => new UserListDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture
                });

            var totalRecords = await shopList.CountAsync();

            var shops = await shopList
                .Take(pageSize)
                .ToListAsync();

            bool hasMore = shops.Count == pageSize;

            int lastId = shops.Any() ? shops.Last().UserId : 0;

            var result = new ScrollDTO<UserListDTO>
            {
                Data = shops,
                TotalRecords = totalRecords,
                LastId = lastId,
                HasMore = hasMore
            };

            _logger.LogInformation("Listado paginado de talleres");

            return Ok(result);
        }

        [Authorize]
        [HttpGet("Users/GetUserById/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado");
                return NotFound("Usuario no encontrado.");
            }
            else
            {
                var userDetail = new UserDetailDTO
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture,
                    Name = user.Name,
                    Description = user.Description,
                    Adress = user.Adress,
                    CountFollowers = user.CountFollowers,
                    CountFollowing = user.CountFollowing,
                    Type = user.Type
                   
                };

                _logger.LogInformation("Detalles del usuario");

                return Ok(userDetail); 
            }
        }

        [Authorize]
        [HttpPost("UserCreate")]
        public async Task<IActionResult> CreateUser([FromForm] UserCreateDTO userCreateDto, IFormFile profilePicture)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int userTypeClaim = int.Parse(User.FindFirst("UserType")?.Value);
            UserType userType = (UserType)userTypeClaim;

            long maxSize = 5 * 1024 * 1024;
            string targetFolder = "profile_images"; 

          
            if (string.IsNullOrEmpty(userCreateDto.Name) || string.IsNullOrEmpty(userCreateDto.UserName))
                return BadRequest("El nombre y el nombre de usuario son obligatorios.");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userCreateDto.UserName);

            if (existingUser != null)
                return Conflict("El nombre de usuario ya está en uso.");

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileExtension = Path.GetExtension(profilePicture.FileName);

                if (!_fileFolderService.IsValidFileExtension(fileExtension))
                    return BadRequest("Solo se permiten archivos JPG o PNG.");

                if (!_fileFolderService.IsValidFileSize(profilePicture.Length, maxSize))
                    return BadRequest("El tamaño del archivo no debe exceder los 5 MB.");


                var profilePicturePath = await _fileFolderService.SaveFileAsync(profilePicture, userId, targetFolder);

                if (profilePicturePath == null)
                {
                    _logger.LogError("No se pudo guardar la imagen del perfil.");
                    return BadRequest("Hubo un error al guardar la imagen.");
                }

                var newUser = new User
                {
                    UserId = userId,
                    ProfilePicture = profilePicturePath,
                    Name = userCreateDto.Name,
                    UserName = userCreateDto.UserName,
                    Description = userCreateDto.Description,
                    Adress = userCreateDto.Address,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Type = userType
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Nuevo usuario creado");

                return Ok();
            }
            else
            {
                _logger.LogWarning("No ha subido imagen.");
                return BadRequest("No ha subido imagen.");
            }
        }

        [Authorize]
        [HttpPatch("UserUpdate")]
        public async Task<IActionResult> UpdateUser([FromForm] UserCreateDTO userDTO, IFormFile profilePicture)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();
                    long maxSize = 5 * 1024 * 1024; // 5 MB

                    if (!_fileFolderService.IsValidFileExtension(fileExtension))
                        return BadRequest("Solo se permiten archivos JPG o PNG.");

                    if (!_fileFolderService.IsValidFileSize(profilePicture.Length, maxSize))
                        return BadRequest("El tamaño del archivo no debe exceder los 5 MB.");

                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), user.ProfilePicture.TrimStart('/'));
                        if (!_fileFolderService.DeleteFile(oldImagePath))  
                            return BadRequest("Hubo un problema al intentar eliminar la imagen anterior.");
                    }
                    var newFilePath = await _fileFolderService.SaveFileAsync(profilePicture, userId, "profile_images");
                    if (newFilePath == null)
                        return BadRequest("Hubo un error al guardar la imagen.");

                    user.ProfilePicture = newFilePath; 
                }

                user.Name = userDTO.Name;
                user.UserName = userDTO.UserName;
                user.Description = userDTO.Description;
                user.Adress = userDTO.Address;
                user.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Perfil actualizado");
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Perfil actualizado");
            }
            else
            {
                _logger.LogWarning("El perfil no existe");
                return NotFound();
            }
        }





    }
}
