﻿
namespace OpenWeatherMapClient.Interfaces;
public interface IRestClientBase
{
    void Dispose();
    string AppName { get; set; }
    string BaseAPIUrl { get; set; }
    bool IsError { get; set; }
    string Status { get; set; }
}
