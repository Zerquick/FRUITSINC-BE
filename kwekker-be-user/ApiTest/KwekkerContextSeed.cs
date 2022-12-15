using System;
using System.Linq;
using Api.Data;
using Api.Models;

namespace ApiTest;

public class KwekkerContextSeed
{
    private readonly KwekkerContext _context;

    public KwekkerContextSeed(KwekkerContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (_context.Users.Any())
        {
            return;
        }

        var user = new TestUser();
        
        _context.Users.Add(user);

        var kwek = new Kwek
        {
            Text = "Hello World",
            User = user,
            PostedAt = DateTime.Now - TimeSpan.FromMinutes(5)
        };
        
        var kwek2 = new Kwek
        {
            Text = "Second post!",
            User = user,
            PostedAt = DateTime.Now
        };

        _context.Kweks.Add(kwek);
        _context.Kweks.Add(kwek2);
        
        _context.SaveChanges();
    }
}