using System;

namespace UnsortedOrderer.Contracts.Services;

public interface IVideoDateService
{
    DateTime GetVideoDate(string filePath);
}
