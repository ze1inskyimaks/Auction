using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auction.API.ApiControllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoryApi : ControllerBase
{
    private readonly IAuctionCategoryRepository _categoryRepository;
    private readonly ICategoryRequestRepository _categoryRequestRepository;
    private readonly UserManager<Account> _userManager;

    public CategoryApi(
        IAuctionCategoryRepository categoryRepository,
        ICategoryRequestRepository categoryRequestRepository,
        UserManager<Account> userManager)
    {
        _categoryRepository = categoryRepository;
        _categoryRequestRepository = categoryRequestRepository;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveCategories()
    {
        var categories = await _categoryRepository.GetActiveCategories();
        return Ok(categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description
        }));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Category name is required." });
        }

        var existing = await _categoryRepository.GetByName(dto.Name);
        if (existing is not null)
        {
            return BadRequest(new { message = "Category with this name already exists." });
        }

        var category = await _categoryRepository.Create(new AuctionCategory
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        });

        return Ok(new { category.Id, category.Name, category.Description });
    }

    [Authorize(Roles = "USER")]
    [HttpPost("requests")]
    public async Task<IActionResult> CreateCategoryRequest([FromBody] CreateCategoryRequestDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Category name is required." });
        }

        var existingCategory = await _categoryRepository.GetByName(dto.Name);
        if (existingCategory is not null)
        {
            return BadRequest(new { message = "Category already exists. Please select it from the list." });
        }

        var pendingRequest = await _categoryRequestRepository.GetPendingByName(dto.Name);
        if (pendingRequest is not null)
        {
            return BadRequest(new { message = "Request for this category is already pending review." });
        }

        var request = await _categoryRequestRepository.Create(new CategoryRequest
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            RequestedById = user.Id
        });

        return Ok(new { request.Id, request.Name, request.Status });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("requests")]
    public async Task<IActionResult> GetCategoryRequests()
    {
        var requests = await _categoryRequestRepository.GetAll();
        return Ok(requests.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.Status,
            r.AdminComment,
            r.CategoryId,
            RequestedBy = new
            {
                r.RequestedById,
                UserName = r.RequestedBy?.UserName,
                Email = r.RequestedBy?.Email
            },
            ReviewedBy = r.ReviewedById == null
                ? null
                : new
                {
                    r.ReviewedById,
                    UserName = r.ReviewedBy?.UserName,
                    Email = r.ReviewedBy?.Email
                },
            r.CreatedAt,
            r.ReviewedAt
        }));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveCategoryRequest(Guid id, [FromBody] ReviewCategoryRequestDto? dto = null)
    {
        var admin = await _userManager.GetUserAsync(User);
        if (admin is null)
        {
            return Unauthorized();
        }

        var request = await _categoryRequestRepository.GetById(id);
        if (request is null)
        {
            return NotFound(new { message = "Category request not found." });
        }

        if (request.Status != CategoryRequestStatus.Pending)
        {
            return BadRequest(new { message = "Only pending requests can be approved." });
        }

        var category = await _categoryRepository.GetByName(request.Name);
        if (category is null)
        {
            category = await _categoryRepository.Create(new AuctionCategory
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            });
        }

        request.Status = CategoryRequestStatus.Approved;
        request.CategoryId = category.Id;
        request.AdminComment = dto?.AdminComment?.Trim();
        request.ReviewedById = admin.Id;
        request.ReviewedAt = DateTime.UtcNow;
        await _categoryRequestRepository.Update(request);

        return Ok(new { request.Id, request.Status, request.CategoryId });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectCategoryRequest(Guid id, [FromBody] ReviewCategoryRequestDto? dto = null)
    {
        var admin = await _userManager.GetUserAsync(User);
        if (admin is null)
        {
            return Unauthorized();
        }

        var request = await _categoryRequestRepository.GetById(id);
        if (request is null)
        {
            return NotFound(new { message = "Category request not found." });
        }

        if (request.Status != CategoryRequestStatus.Pending)
        {
            return BadRequest(new { message = "Only pending requests can be rejected." });
        }

        request.Status = CategoryRequestStatus.Rejected;
        request.AdminComment = dto?.AdminComment?.Trim();
        request.ReviewedById = admin.Id;
        request.ReviewedAt = DateTime.UtcNow;
        await _categoryRequestRepository.Update(request);

        return Ok(new { request.Id, request.Status });
    }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ReviewCategoryRequestDto
{
    public string? AdminComment { get; set; }
}
