using BackendApi.DTOs.Users;
using BackendApi.Models;
using BackendApi.Repositories;

namespace BackendApi.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(long userId);
    Task<UserProfileDto?> UpdateUserProfileAsync(long userId, UpdateProfileRequest request);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public UserService(IUserRepository userRepository, IUserProfileRepository userProfileRepository)
    {
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(long userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var profile = await _userProfileRepository.GetByUserIdAsync(userId);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = profile?.FullName ?? string.Empty,
            DateOfBirth = profile?.DateOfBirth == default ? null : profile?.DateOfBirth,
            PhoneNumber = profile?.PhoneNumber ?? string.Empty,
            PublicCode = $"USER{user.Id:D5}"
        };
    }

    public async Task<UserProfileDto?> UpdateUserProfileAsync(long userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth ?? default,
                PhoneNumber = request.PhoneNumber,
            };
            await _userProfileRepository.CreateAsync(profile);
        }
        else
        {
            profile.FullName = request.FullName;
            profile.DateOfBirth = request.DateOfBirth ?? default;
            profile.PhoneNumber = request.PhoneNumber;
            await _userProfileRepository.UpdateAsync(profile);
        }

        return await GetUserProfileAsync(userId);
    }
}
