﻿namespace RPGCompanion.Server.Infrastructure.DataTransferObjects
{
    public class RegisterDTO
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
