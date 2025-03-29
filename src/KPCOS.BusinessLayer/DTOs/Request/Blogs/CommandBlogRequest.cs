using System;
using System.ComponentModel.DataAnnotations;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.BusinessLayer.DTOs.Request.Blogs;

/// <summary>
/// Request model for creating or updating a blog post
/// </summary>
/// <example>
/// Sample JSON for creating a koi pond construction blog:
/// {
///   "name": "Modern Koi Pond Construction Techniques",
///   "description": "A comprehensive guide to modern techniques for constructing beautiful and functional koi ponds in residential gardens",
///   "type": "PROJECT",
///   "no": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///   "isActive": true
/// }
/// </example>
public class CommandBlogRequest
{
    /// <summary>
    /// The title or name of the blog post
    /// </summary>
    /// <example>Advanced Koi Pond Filtration Systems</example>
    public string? Name { get; set; }

    /// <summary>
    /// The detailed content or description of the blog post
    /// </summary>
    /// <example>This guide covers various filtration systems for koi ponds, including biological, mechanical, and UV filtration options.</example>
    public string? Description { get; set; }

    /// <summary>
    /// The type of blog post, which determines its relation to other entities
    /// </summary>
    /// <example>PROJECT</example>
    [EnumDataType(typeof(EnumBlogType))]
    public string? Type { get; set; }

    /// <summary>
    /// The identifier of the related entity (Project, Package, or MaintenancePackage)
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? No { get; set; }

    /// <summary>
    /// Indicates whether the blog post should be active and visible
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }
}
