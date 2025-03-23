using System;
using KPCOS.BusinessLayer.DTOs.Request.Docs;

namespace KPCOS.BusinessLayer.Services;

public interface IDocService
{
    /// <summary>
    /// Creates a new document for a project
    /// </summary>
    /// <param name="projectId">The ID of the project to add a document to</param>
    /// <param name="request">The document data to create</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>This method creates a new document associated with a project.</para>
    /// <para>Document names must be unique within a project.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project or document type is not found</exception>
    /// <exception cref="BadRequestException">Thrown when document with the same name already exists in the project</exception>
    Task CreateDocAsync(CommandDocRequest request);

    /// <summary>
    /// Updates an existing document for a project
    /// </summary>
    /// <param name="projectId">The ID of the project the document belongs to</param>
    /// <param name="docId">The ID of the document to update</param>
    /// <param name="request">The updated document data</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>This method updates an existing document associated with a project.</para>
    /// <para>Document names must be unique within a project.</para>
    /// <para>The ProjectId field in the request is ignored to ensure the document remains associated with the original project.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when project, document, or document type is not found</exception>
    /// <exception cref="BadRequestException">Thrown when document with the same name already exists in the project</exception>
    Task UpdateDocAsync(Guid docId, CommandDocRequest request);
    
    /// <summary>
    /// Initiates the document acceptance process by generating an OTP
    /// </summary>
    /// <param name="docId">Document ID to accept</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>This method creates an OTP and sends it to the user's email.</para>
    /// <para>The OTP is valid for 5 minutes.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when document or user is not found</exception>
    Task AcceptDocAsync(Guid docId);

    /// <summary>
    /// Verifies the document using the OTP received in email
    /// </summary>
    /// <param name="docId">Document ID to verify</param>
    /// <param name="otp">OTP code received in email</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>This method verifies the OTP and marks the document as verified.</para>
    /// <para>The OTP must be used within 5 minutes of generation.</para>
    /// </remarks>
    /// <exception cref="NotFoundException">Thrown when document is not found or OTP is invalid</exception>
    Task VerifyDocAsync(Guid docId, string otp);
}
