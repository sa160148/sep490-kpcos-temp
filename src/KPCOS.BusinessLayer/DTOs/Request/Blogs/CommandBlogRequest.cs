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
///   "no": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///   "isActive": true
/// }
/// </example>
/// <remarks>
/// The Type property is optional. If not provided, it will be automatically determined based on the No value:
/// - If No refers to a Project, Type will be set to PROJECT
/// - If No refers to a Package, Type will be set to PACKAGE
/// - If No refers to a MaintenancePackage, Type will be set to MAINTENANCE_PACKAGE
/// - If No is null or doesn't match any entity, Type will be set to OTHER
/// </remarks>
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
    /// The type of blog post (optional, will be determined automatically)
    /// </summary>
    /// <remarks>
    /// This property is optional and will be ignored. The type will be determined automatically based on the No value.
    /// </remarks>
    /// <example>PROJECT</example>
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
