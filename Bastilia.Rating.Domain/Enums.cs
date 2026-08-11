namespace Bastilia.Rating.Domain;

public enum ProjectType
{
    MasteringGame,
    NearRpgProject,
    TeamPlaying,
    InternalProject,
    ActivityOnConvent,
}

public enum BrandType
{
    Bastilia,
    PubliclySupported,
    PrivatelySupported,
    JustTracked,
}

public enum BastiliaStatusType
{
    Member,
    President,
}

public enum ProjectLevel
{
    XS = 0, // ачивка = 1 балл
    S = 1, // ачивки = 3, 1 балл
    M = 2, // ачивки = 5, 3, 1 балл
    L = 3, // ачивки = 7, 4, 1 балл
}