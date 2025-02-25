using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using KPCOS.BusinessLayer.DTOs.Response;
using Microsoft.Extensions.Configuration;

namespace KPCOS.BusinessLayer.Services.Implements;

public class FirebaseService
{
    private readonly IConfiguration _config;
    private readonly FirestoreDb _dbFirestore;
    private static FirebaseApp? _firebaseApp;
    private readonly IMapper _mapper;

    public FirebaseService(IConfiguration config, IMapper mapper)
    {
        _config = config;
        _mapper = mapper;
        if (_firebaseApp == null)
        {
            string authJsonFile = _config["FirebaseSettings:ConfigFile"];
            var appOptions = new AppOptions()
            {
                Credential = GoogleCredential.FromFile(authJsonFile)
            };

            _firebaseApp = FirebaseApp.Create(appOptions);
        }
        string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase_app_settings.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        _dbFirestore = FirestoreDb.Create("roomspt-37b2f");
    }
    
    public async Task<string> SaveUser(DataAccessLayer.Entities.User saveUser, Guid id, string collectionName)
    {
        try
        {
            var saveUserResponse = _mapper.Map<UserResponse>(saveUser);
            DocumentReference docRef = _dbFirestore.Collection(collectionName).Document(id.ToString());
            Console.WriteLine("docRef: " + docRef);
            await docRef.SetAsync(saveUserResponse);
            return (await docRef.GetSnapshotAsync()).UpdateTime.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving document: {e.Message}");
            throw;
        }
    }
}