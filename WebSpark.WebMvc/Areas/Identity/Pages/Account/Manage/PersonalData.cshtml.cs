﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace WebSpark.WebMvc.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel(
        UserManager<UserIdentity.Data.WebSparkUser> userManager,
        ILogger<PersonalDataModel> logger) : PageModel
    {
        private readonly UserManager<UserIdentity.Data.WebSparkUser> _userManager = userManager;
        private readonly ILogger<PersonalDataModel> _logger = logger;

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }
    }
}
