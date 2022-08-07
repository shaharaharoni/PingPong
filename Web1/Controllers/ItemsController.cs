using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Web1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        public readonly Cache cache;
        private readonly IStorageService _storageService;

        public ItemsController(IStorageService storageService, IDistributedCache cacheProvider)
        {
            cache = new Cache(cacheProvider);
            _storageService = storageService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] Item item)
        {
            try
            {
                await _storageService.AddAsync(item);
                return CreatedAtAction(nameof(GetAsync), new { id = item.Id }, item);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                await _storageService.DeleteAsync(id);

                // Remove from Cache the deleted item
                await cache.Clear(id);
                return NoContent();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            try
            {
                //Check if item is in cache, if not add it  
                Item item = await cache.Get(id);
                if (item is null)
                {
                    item = await _storageService.GetAsync(id);
                    await cache.Set(id, item);
                    return Ok(new { LoadedFromRedis = false, Data = item });
                }
                return Ok(new { LoadedFromRedis = true, Data = item });
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([FromBody] Item item)
        {
            try
            {
                await _storageService.UpdateAsync(item.Id, item);
                //Remove from cache the updated item
                await cache.Clear(item.Id);
                await cache.Set(item.Id, item);
                return NoContent();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await _storageService.GetAllAsync());
            }
            catch
            {
                return BadRequest();
            }
        }

    }
}