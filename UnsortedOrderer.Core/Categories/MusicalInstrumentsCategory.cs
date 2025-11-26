namespace UnsortedOrderer.Categories;

public sealed class MusicalInstrumentsCategory : FileCategory
{
    private static readonly string[] InstrumentExtensions =
    [
        ".gp", ".gpx", ".gp5", ".gp4", ".gp3", ".mid", ".midi"
    ];

    public MusicalInstrumentsCategory(string folderName)
        : base("Musical Instruments", folderName, InstrumentExtensions)
    {
    }
}
