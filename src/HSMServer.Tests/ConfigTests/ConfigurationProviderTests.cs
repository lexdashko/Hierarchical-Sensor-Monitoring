﻿using HSMServer.Configuration;
using HSMServer.DataLayer;
using Moq;
using Xunit;

namespace HSMServer.Tests.ConfigTests
{
    public class ConfigurationProviderTests
    {
        //[Fact]
        //public void ReturnDefaultConfigObjectIfDoesNotExist()
        //{
        //    //Arrange
        //    var databaseMock = new Mock<IDatabaseClass>();
        //    databaseMock.Setup(db => db.ReadConfigurationObject()).Returns(() => null);
        //    IConfigurationProvider configurationProvider = new ConfigurationProvider(databaseMock.Object);

        //    //Act
        //    var configurationObject = configurationProvider.CurrentConfigurationObject;

        //    //Assert
        //    var defaultObj = ConfigurationObject.CreateDefaultObject();
        //    Assert.True(configurationObject.Equals(defaultObj));
        //}

        //[Fact]
        //public void ReturnNewConfigurationObjectAfterUpdate()
        //{
        //    //Arrange
        //    var databaseMock = new Mock<IDatabaseClass>();
        //    databaseMock.Setup(db => db.ReadConfigurationObject()).Returns(() => null);
        //    IConfigurationProvider configurationProvider = new ConfigurationProvider(databaseMock.Object);
        //    ConfigurationObject newObj = new ConfigurationObject() { MaxPathLength = 34 };

        //    //Act
        //    configurationProvider.UpdateConfigurationObject(newObj);

        //    //Assert
        //    var currentObj = configurationProvider.CurrentConfigurationObject;
        //    Assert.Equal(newObj, currentObj);
        //}
    }
}
