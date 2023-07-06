using System.Linq.Expressions;
using System.Net;
using System.Reflection.PortableExecutable;
using HangfireDotNetCoreExample.Features.DevCodes;
using LiteDB;
using Microsoft.Extensions.Options;
using HostingEnvironmentExtensions = Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions;

namespace HangfireDotNetCoreExample.DbService;

public class LiteDbService
{
    private LiteDatabase _db;

    public LiteDbService(IOptions<LiteDbOption> option)
    {
        CreateConnection(option);
    }

    public T GetOne<T>(Expression<Func<T, bool>> expression)
    {
        return _db.GetCollection<T>().FindOne(expression);
    }

    public List<T> GetList<T>()
    {
        return _db.GetCollection<T>()
            .FindAll()
            .ToList();
    }

    public List<T> GetPagination<T>(int pageNo,
        int pageSize,
        Expression<Func<T, bool>> predicate)
    {
        int skip = (pageNo - 1) * pageSize;
        var list = _db.GetCollection<T>()
            .Find(predicate, skip, pageSize)
            .ToList();

        return list;
    }

    public int GetTotalRowCount<T>(Expression<Func<T, bool>> predicate)
    {
        return _db.GetCollection<T>().Count(predicate);
    }

    public void Insert<T>(T model)
    {
        _db.GetCollection<T>()
            .Insert(model);
    }

    public bool Update<T>(T model)
    {
        return _db.GetCollection<T>()
            .Update(model);
    }

    public bool Delete<T>(BsonValue id)
    {
        return _db.GetCollection<T>()
            .Delete(id);
    }

    private void CreateConnection(IOptions<LiteDbOption> option)
    {
        string dbName = "app.db";
        string folderPath = GetDbFolderPath(option);

        DeleteDbFile(folderPath);

        if (!folderPath.IsNullOrEmpty() && Directory.Exists(folderPath))
            _db = new LiteDatabase(folderPath + $"/{dbName}");
    }

    private void DeleteDbFile(string folderPath)
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private string GetDbFolderPath(IOptions<LiteDbOption> option)
    {
        string folderPath = "";
        try
        {
            folderPath = Path.Combine(
                AppDomain
                    .CurrentDomain
                    .BaseDirectory, option.Value.DatabaseLocation);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return folderPath;
    }
}

public class LiteDbOption
{
    public string DatabaseLocation { get; set; }
}