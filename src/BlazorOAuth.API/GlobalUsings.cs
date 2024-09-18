﻿global using Google.Apis.Auth;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Mvc;
global using System.Security.Claims;
global using BlazorOAuth.API.Utils;
global using System.Text;
global using BlazorOAuth.API.Options;
global using Microsoft.Extensions.Options;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authorization;
global using System.IdentityModel.Tokens.Jwt;
global using BlazorOAuth.API.Database;
global using BlazorOAuth.API.Models.Contracts;
global using BlazorOAuth.API.Models.Commands;
global using BlazorOAuth.API.Services;
global using BlazorOAuth.API.Models.Entities;
global using AuthenticationOptions = BlazorOAuth.API.Options.AuthenticationOptions;
global using AuthorizationOptions = BlazorOAuth.API.Options.AuthorizationOptions;