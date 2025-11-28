using System;

namespace UnsortedOrderer.Application.Contracts.Services.Categories;

public interface IVideoDateService
{
    DateTime GetVideoDate(string filePath);
}
