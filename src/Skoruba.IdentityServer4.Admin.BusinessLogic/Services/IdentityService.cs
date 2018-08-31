﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Dtos.Common;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Dtos.Identity;
using Skoruba.IdentityServer4.Admin.BusinessLogic.ExceptionHandling;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Repositories.Interfaces;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Resources;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Services.Interfaces;

namespace Skoruba.IdentityServer4.Admin.BusinessLogic.Services
{
    public class IdentityService<TIdentityDbContext, TUserDto, TUserDtoKey, TRoleDto, TRoleDtoKey, TClaimDtoKey, TUserKey, TRoleKey, TClaimKey, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        : IIdentityService<TIdentityDbContext, TUserDto, TUserDtoKey, TRoleDto, TRoleDtoKey, TClaimDtoKey, TUserKey, TRoleKey, TClaimKey, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TIdentityDbContext : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TUserDto : UserDto<TUserDtoKey>
        where TRoleDto : RoleDto<TRoleDtoKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
        private readonly IIdentityRepository<TIdentityDbContext, TUserKey, TRoleKey, TClaimKey, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> _identityRepository;
        private readonly IIdentityServiceResources _identityServiceResources;
        private readonly IMapper _mapper;

        public IdentityService(IIdentityRepository<TIdentityDbContext, TUserKey, TRoleKey, TClaimKey, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> identityRepository,
            IIdentityServiceResources identityServiceResources,
            IMapper mapper)
        {
            _identityRepository = identityRepository;
            _identityServiceResources = identityServiceResources;
            _mapper = mapper;
        }

        public async Task<bool> ExistsUserAsync(string userId)
        {
            var exists = await _identityRepository.ExistsUserAsync(userId);
            if (!exists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            return true;
        }

        public async Task<bool> ExistsRoleAsync(string roleId)
        {
            var exists = await _identityRepository.ExistsRoleAsync(roleId);
            if (!exists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.RoleDoesNotExist().Description, roleId), _identityServiceResources.RoleDoesNotExist().Description);

            return true;
        }

        public async Task<UsersDto<TUserDto, TUserDtoKey>> GetUsersAsync(string search, int page = 1, int pageSize = 10)
        {
            var pagedList = await _identityRepository.GetUsersAsync(search, page, pageSize);
            var usersDto = _mapper.Map<UsersDto<TUserDto, TUserDtoKey>>(pagedList);

            return usersDto;
        }

        public async Task<RolesDto<TRoleDto, TRoleDtoKey>> GetRolesAsync(string search, int page = 1, int pageSize = 10)
        {
            PagedList<TRole> pagedList = await _identityRepository.GetRolesAsync(search, page, pageSize);
            var rolesDto = _mapper.Map<RolesDto<TRoleDto, TRoleDtoKey>>(pagedList);

            return rolesDto;
        }

        public async Task<IdentityResult> CreateRoleAsync(TRoleDto role)
        {
            var roleEntity = _mapper.Map<TRole>(role);
            var identityResult = await _identityRepository.CreateRoleAsync(roleEntity);

            return HandleIdentityError(identityResult, _identityServiceResources.RoleCreateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, role);
        }

        private IdentityResult HandleIdentityError(IdentityResult identityResult, string errorMessage, string errorKey, object model)
        {
            if (!identityResult.Errors.Any()) return identityResult;
            var viewErrorMessages = _mapper.Map<List<ViewErrorMessage>>(identityResult.Errors);

            throw new UserFriendlyViewException(errorMessage, errorKey, viewErrorMessages, model);
        }

        public async Task<TRoleDto> GetRoleAsync(TRoleDto role)
        {
            var roleEntity = _mapper.Map<TRole>(role);

            var userIdentityRole = await _identityRepository.GetRoleAsync(roleEntity);
            if (userIdentityRole == null) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.RoleDoesNotExist().Description, role.Id), _identityServiceResources.RoleDoesNotExist().Description);

            var roleDto = _mapper.Map<TRoleDto>(userIdentityRole);

            return roleDto;
        }

        public async Task<List<TRoleDto>> GetRolesAsync()
        {
            var roles = await _identityRepository.GetRolesAsync();
            var roleDtos = _mapper.Map<List<TRoleDto>>(roles);

            return roleDtos;
        }

        public async Task<IdentityResult> UpdateRoleAsync(TRoleDto role)
        {
            var userIdentityRole = _mapper.Map<TRole>(role);
            var identityResult = await _identityRepository.UpdateRoleAsync(userIdentityRole);

            return HandleIdentityError(identityResult, _identityServiceResources.RoleUpdateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, role);
        }

        public async Task<TUserDto> GetUserAsync(string userId)
        {
            var identity = await _identityRepository.GetUserAsync(userId);
            if (identity == null) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            var userDto = _mapper.Map<TUserDto>(identity);

            return userDto;
        }

        public async Task<IdentityResult> CreateUserAsync(TUserDto user)
        {
            var userIdentity = _mapper.Map<TUser>(user);
            var identityResult = await _identityRepository.CreateUserAsync(userIdentity);

            return HandleIdentityError(identityResult, _identityServiceResources.UserCreateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, user);
        }

        public async Task<IdentityResult> UpdateUserAsync(TUserDto user)
        {
            var userIdentity = _mapper.Map<TUser>(user);
            var identityResult = await _identityRepository.UpdateUserAsync(userIdentity);

            return HandleIdentityError(identityResult, _identityServiceResources.UserUpdateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId, TUserDto user)
        {
            var identityResult = await _identityRepository.DeleteUserAsync(userId);

            return HandleIdentityError(identityResult, _identityServiceResources.UserDeleteFailed().Description, _identityServiceResources.IdentityErrorKey().Description, user);
        }

        public async Task<IdentityResult> CreateUserRoleAsync(UserRolesDto<TRoleDto, TUserDtoKey, TRoleDtoKey> role)
        {
            var identityResult = await _identityRepository.CreateUserRoleAsync(role.UserId.ToString(), role.RoleId.ToString());

            if (!identityResult.Errors.Any()) return identityResult;

            var userRolesDto = await BuildUserRolesViewModel(role.UserId, 1);
            return HandleIdentityError(identityResult, _identityServiceResources.UserRoleCreateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, userRolesDto);
        }

        public async Task<UserRolesDto<TRoleDto, TUserDtoKey, TRoleDtoKey>> BuildUserRolesViewModel(TUserDtoKey id, int? page)
        {
            var roles = await GetRolesAsync();
            var userRoles = await GetUserRolesAsync(id.ToString(), page ?? 1);
            userRoles.UserId = id;
            userRoles.RolesList = roles.Select(x => new SelectItem(x.Id.ToString(), x.Name)).ToList();

            return userRoles;
        }

        public async Task<UserRolesDto<TRoleDto, TUserDtoKey, TRoleDtoKey>> GetUserRolesAsync(string userId, int page = 1, int pageSize = 10)
        {
            var userExists = await _identityRepository.ExistsUserAsync(userId);
            if (!userExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            var userIdentityRoles = await _identityRepository.GetUserRolesAsync(userId, page, pageSize);
            var roleDtos = _mapper.Map<UserRolesDto<TRoleDto, TUserDtoKey, TRoleDtoKey>>(userIdentityRoles);

            return roleDtos;
        }

        public async Task<IdentityResult> DeleteUserRoleAsync(UserRolesDto<TRoleDto, TUserDtoKey, TRoleDtoKey> role)
        {
            var identityResult = await _identityRepository.DeleteUserRoleAsync(role.UserId.ToString(), role.RoleId.ToString());

            return HandleIdentityError(identityResult, _identityServiceResources.UserRoleDeleteFailed().Description, _identityServiceResources.IdentityErrorKey().Description, role);
        }

        public async Task<UserClaimsDto<TUserDtoKey, TClaimDtoKey>> GetUserClaimsAsync(string userId, int page = 1, int pageSize = 10)
        {
            var userExists = await _identityRepository.ExistsUserAsync(userId);
            if (!userExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            var identityUserClaims = await _identityRepository.GetUserClaimsAsync(userId, page, pageSize);
            var claimDtos = _mapper.Map<UserClaimsDto<TUserDtoKey, TClaimDtoKey>>(identityUserClaims);

            return claimDtos;
        }

        public async Task<UserClaimsDto<TUserDtoKey, TClaimDtoKey>> GetUserClaimAsync(string userId, string claimId)
        {
            var userExists = await _identityRepository.ExistsUserAsync(userId);
            if (!userExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            var identityUserClaim = await _identityRepository.GetUserClaimAsync(userId, claimId);
            if (identityUserClaim == null) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserClaimDoesNotExist().Description, userId), _identityServiceResources.UserClaimDoesNotExist().Description);

            var userClaimsDto = _mapper.Map<UserClaimsDto<TUserDtoKey, TClaimDtoKey>>(identityUserClaim);

            return userClaimsDto;
        }

        public async Task<IdentityResult> CreateUserClaimsAsync(UserClaimsDto<TUserDtoKey, TClaimDtoKey> claimsDto)
        {
            var userIdentityUserClaim = _mapper.Map<TUserClaim>(claimsDto);
            var identityResult = await _identityRepository.CreateUserClaimsAsync(userIdentityUserClaim);

            return HandleIdentityError(identityResult, _identityServiceResources.UserClaimsCreateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, claimsDto);
        }

        public async Task<int> DeleteUserClaimsAsync(UserClaimsDto<TUserDtoKey, TClaimDtoKey> claim)
        {
            return await _identityRepository.DeleteUserClaimsAsync(claim.UserId.ToString(), claim.ClaimId.ToString());
        }

        public virtual TUserDtoKey ConvertUserDtoKeyFromString(string id)
        {
            if (id == null)
            {
                return default(TUserDtoKey);
            }
            return (TUserDtoKey)TypeDescriptor.GetConverter(typeof(TUserDtoKey)).ConvertFromInvariantString(id);
        }

        public async Task<UserProvidersDto<TUserDtoKey>> GetUserProvidersAsync(string userId)
        {
            var userExists = await _identityRepository.ExistsUserAsync(userId);
            if (!userExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            var userLoginInfos = await _identityRepository.GetUserProvidersAsync(userId);
            var providersDto = _mapper.Map<UserProvidersDto<TUserDtoKey>>(userLoginInfos);
            providersDto.UserId = ConvertUserDtoKeyFromString(userId);

            return providersDto;
        }

        public async Task<IdentityResult> DeleteUserProvidersAsync(UserProviderDto<TUserDtoKey> provider)
        {
            var identityResult = await _identityRepository.DeleteUserProvidersAsync(provider.UserId.ToString(), provider.ProviderKey, provider.LoginProvider);

            return HandleIdentityError(identityResult, _identityServiceResources.UserProviderDeleteFailed().Description, _identityServiceResources.IdentityErrorKey().Description, provider);
        }

        public async Task<UserProviderDto<TUserDtoKey>> GetUserProviderAsync(string userId, string providerKey)
        {
            var userExists = await _identityRepository.ExistsUserAsync(userId);
            if (!userExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userId), _identityServiceResources.UserDoesNotExist().Description);

            var identityUserLogin = await _identityRepository.GetUserProviderAsync(userId, providerKey);
            if (identityUserLogin == null) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserProviderDoesNotExist().Description, providerKey), _identityServiceResources.UserProviderDoesNotExist().Description);

            var userProviderDto = _mapper.Map<UserProviderDto<TUserDtoKey>>(identityUserLogin);

            return userProviderDto;
        }

        public async Task<IdentityResult> UserChangePasswordAsync(UserChangePasswordDto<TUserDtoKey> userPassword)
        {
            var userExists = await _identityRepository.ExistsUserAsync(userPassword.UserId.ToString());
            if (!userExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.UserDoesNotExist().Description, userPassword.UserId), _identityServiceResources.UserDoesNotExist().Description);

            var identityResult = await _identityRepository.UserChangePasswordAsync(userPassword.UserId.ToString(), userPassword.Password);

            return HandleIdentityError(identityResult, _identityServiceResources.UserChangePasswordFailed().Description, _identityServiceResources.IdentityErrorKey().Description, userPassword);
        }

        public async Task<IdentityResult> CreateRoleClaimsAsync(RoleClaimsDto<TRoleDtoKey, TClaimDtoKey> claimsDto)
        {
            var identityRoleClaim = _mapper.Map<TRoleClaim>(claimsDto);
            var identityResult = await _identityRepository.CreateRoleClaimsAsync(identityRoleClaim);

            return HandleIdentityError(identityResult, _identityServiceResources.RoleClaimsCreateFailed().Description, _identityServiceResources.IdentityErrorKey().Description, claimsDto);
        }

        public async Task<RoleClaimsDto<TRoleDtoKey, TClaimDtoKey>> GetRoleClaimsAsync(string roleId, int page = 1, int pageSize = 10)
        {
            var roleExists = await _identityRepository.ExistsRoleAsync(roleId);
            if (!roleExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.RoleDoesNotExist().Description, roleId), _identityServiceResources.RoleDoesNotExist().Description);

            var identityRoleClaims = await _identityRepository.GetRoleClaimsAsync(roleId, page, pageSize);
            var roleClaimDtos = _mapper.Map<RoleClaimsDto<TRoleDtoKey, TClaimDtoKey>>(identityRoleClaims);

            return roleClaimDtos;
        }

        public async Task<RoleClaimsDto<TRoleDtoKey, TClaimDtoKey>> GetRoleClaimAsync(string roleId, string claimId)
        {
            var roleExists = await _identityRepository.ExistsRoleAsync(roleId);
            if (!roleExists) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.RoleDoesNotExist().Description, roleId), _identityServiceResources.RoleDoesNotExist().Description);

            var identityRoleClaim = await _identityRepository.GetRoleClaimAsync(roleId, claimId);
            if (identityRoleClaim == null) throw new UserFriendlyErrorPageException(string.Format(_identityServiceResources.RoleClaimDoesNotExist().Description, claimId), _identityServiceResources.RoleClaimDoesNotExist().Description);
            var roleClaimsDto = _mapper.Map<RoleClaimsDto<TRoleDtoKey, TClaimDtoKey>>(identityRoleClaim);

            return roleClaimsDto;
        }

        public async Task<int> DeleteRoleClaimsAsync(RoleClaimsDto<TRoleDtoKey, TClaimDtoKey> role)
        {
            return await _identityRepository.DeleteRoleClaimsAsync(role.RoleId.ToString(), role.ClaimId.ToString());
        }

        public async Task<IdentityResult> DeleteRoleAsync(RoleDto<TRoleDtoKey> role)
        {
            var userIdentityRole = _mapper.Map<TRole>(role);
            var identityResult = await _identityRepository.DeleteRoleAsync(userIdentityRole);

            return HandleIdentityError(identityResult, _identityServiceResources.RoleDeleteFailed().Description, _identityServiceResources.IdentityErrorKey().Description, role);
        }
    }
}