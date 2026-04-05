$baseUrl = "http://localhost:5093/api"
$ErrorActionPreference = 'Stop'

Write-Host "1. Creating Product 1..."
$guid1 = [guid]::NewGuid().ToString()
$sku1 = "TEST-SKU-" + $guid1.Substring(0, 8)
$responseProduct1 = Invoke-RestMethod -Uri "$baseUrl/products" -Method Post -Headers @{"Content-Type"="application/json"} -Body (@{
    sku = $sku1
    name = "Test Product 1"
    description = "A product for testing."
    price = 19.99
} | ConvertTo-Json)
$productId = $responseProduct1.id
Write-Host "Product created: ID $productId"

Write-Host "2. Creating Product 2..."
$guid2 = [guid]::NewGuid().ToString()
$sku2 = "TEST-SKU-" + $guid2.Substring(0, 8)
$responseProduct2 = Invoke-RestMethod -Uri "$baseUrl/products" -Method Post -Headers @{"Content-Type"="application/json"} -Body (@{
    sku = $sku2
    name = "Test Product 2"
    description = "Another product for testing."
    price = 29.99
} | ConvertTo-Json)
$productId2 = $responseProduct2.id
Write-Host "Product created: ID $productId2"

Write-Host "3. Creating Catalog..."
$responseCatalog = Invoke-RestMethod -Uri "$baseUrl/catalogs" -Method Post -Headers @{"Content-Type"="application/json"} -Body (@{
    name = "Summer Catalog"
    description = "Catalog for the Summer season."
} | ConvertTo-Json)
$catalogId = $responseCatalog.id
Write-Host "Catalog created: ID $catalogId"

Write-Host "4. Adding Product 1 to Catalog..."
$addResponse1 = Invoke-RestMethod -Uri "$baseUrl/catalogs/$catalogId/products" -Method Post -Headers @{"Content-Type"="application/json"} -Body (@{
    productIds = @($productId)
} | ConvertTo-Json)
Write-Host "Response (first add): $($addResponse1 | ConvertTo-Json -Depth 5)"

Write-Host "5. Adding BOTH Products to Catalog (simulating a duplicate selection in UI)..."
$addResponse2 = Invoke-RestMethod -Uri "$baseUrl/catalogs/$catalogId/products" -Method Post -Headers @{"Content-Type"="application/json"} -Body (@{
    productIds = @($productId, $productId2)
} | ConvertTo-Json)
Write-Host "Response (second add): $($addResponse2 | ConvertTo-Json -Depth 5)"
