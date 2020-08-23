﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyFakexiecheng.Dtos;
using MyFakexiecheng.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyFakexiecheng.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticateController:ControllerBase

    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticateController(IConfiguration configuration, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)//注入依赖
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]

        public async Task<IActionResult> loginAsync([FromBody] LoginDto loginDto)
        {
            //1 certificate user name and password
            var loginResult = await _signInManager.PasswordSignInAsync(
                loginDto.Email,
                loginDto.Password,
                false,
                false
            );
            if (!loginResult.Succeeded)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByNameAsync(loginDto.Email);
            //2 create jwt
            //header
            var signingAlgorithm = SecurityAlgorithms.HmacSha256;
            //payload
            var claims = new List<Claim>//jwt中自定义payload数据
                                //new[]方括号生成的list不支持.add
            {
                    //sub
                    //new Claim(JwtRegisteredClaimNames.Sub,"fake_user_id"),//fake_user_id用来进行假登录
                    //new Claim(ClaimTypes.Role,"Admin")//lab 11-5 7:30
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                };

            var roleNames = await _userManager.GetRolesAsync(user);//get roles
            foreach (var roleName in roleNames)
            {
                var roleClaim = new Claim(ClaimTypes.Role, roleName);
                claims.Add(roleClaim);
            }

            //signiture

            var secretByte = Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]);
            var signingKey = new SymmetricSecurityKey(secretByte);
            var signingCredentials = new SigningCredentials(signingKey, signingAlgorithm);

            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims,
                notBefore:DateTime.UtcNow,
                expires:DateTime.UtcNow.AddDays(1),
                signingCredentials
                );
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            //3 return 200 ok + jwt
            return Ok(tokenStr);
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // 1 使用用户名创建用户对象
            var user = new ApplicationUser()
            {
                UserName = registerDto.Email,
                Email = registerDto.Email
            };

            // 2 hash密码，保存用户
            var result = await _userManager.CreateAsync(user, registerDto.Password);//hash密码，并连同用户模型对象一起保存到数据库
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            // 3 return
            return Ok();
        }

    }
}
