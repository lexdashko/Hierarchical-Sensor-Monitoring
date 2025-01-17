﻿using HSMServer.Authentication;
using HSMServer.DataLayer.Model;
using System.Collections.Generic;
using System.Linq;

namespace HSMServer.Model.ViewModel
{
    public class EditProductViewModel
    {
        public string ProductName { get; set; }
        public string ProductKey { get; set; }
        public List<KeyValuePair<UserViewModel, ProductRoleEnum>> UsersRights { get; set; }

        public List<ExtraKeyViewModel> ExtraKeys { get; set; }

        public EditProductViewModel(Product product, 
            List<KeyValuePair<User, ProductRoleEnum>> usersRights)
        {
            ProductName = product.Name;
            ProductKey = product.Key;
            UsersRights = usersRights.Select(x =>
                new KeyValuePair<UserViewModel, ProductRoleEnum>(
                    new UserViewModel(x.Key), x.Value)).ToList();

            ExtraKeys = product.ExtraKeys?.Select(k => new ExtraKeyViewModel(product.Key, k)).ToList();
        }
    }
}
