# Deploy And Run Script 
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deploying and Running the Application..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

# 1 Ask For Version 
$version = Read-Host "Enter the version to deploy (e.g., v1.0.0)"
if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host "Version cannot be empty. Exiting." -ForegroundColor Red
    exit 1
}

# 2 Ask For Commit Mesage
$commitMessage = Read-Host "Enter the commit message "
if ([string]::IsNullOrWhiteSpace($commitMessage)) {
    $commitMessage = "Release  $version"
}

# 3  Update kubernetes MainFests
write-Host "n[1/4] Updating Kubernetes manifests with version '$version'..." -ForegroundColor Yellow
$files = Get-ChildItem -Path "k8s/*.yaml" 
foreach ($file in $files) {
   $content = Get-Content $file.FullName
    $newContent = $content -replace 'image: (ahmedkhalaf666/social-dotnet-.*):.*?):.*', "image: `$1:$version"
    $newContent | Set-Content $file.FullName
}

# 4 Push To gitHub
Write-Host "n[2/4] Pushing changes and tags to GitHub..." -ForegroundColor Yellow
git add .
git commit -m "$commitMessage"
git tag $version
git push -u origin HEAD --tags

Write-Host "Changes and tags pushed to GitHub successfully." -ForegroundColor Green
write-Host "GitHub Actions Is now building. docker iamges for version '$version'." -ForegroundColor Cyan
write-Host "Please wait for 'Buikd and Push Docker images ' workflow to complete." -ForegroundColor Cyan

# 5 Wait For onfirmation
$confirmation = Read-Host "`n Is The Github Actions Finished? (y/n)"
if ($confirmation -ne 'y') {
    Write-Host "Please wait for the GitHub Actions to finish before running the application." -ForegroundColor Red
    exit 1
}

write-Host "GitHub Actions completed. Proceeding to deploy to Kubernetes..." -ForegroundColor Green
# 6 Todo Deploy To Kubernetes 
write-Host "n[3/4] applying to manifests..." -ForegroundColor Yellow
kubectl apply -f k8s/mongo.yaml
kubectl apply -f k8s/api.yaml
kubectl apply -f k8s/chat.yaml
kubectl apply -f k8s/notification.yaml
kubectl apply -f k8s/frontend.yaml
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/ingress-chat.yaml
kubectl apply -f k8s/ingress-notification.yaml

# 7 Forec Update 
write-Host "n[4/4] Forcing  rollout..." -ForegroundColor Yellow
kubectl rollout restart deployment/api 
kubectl rollout restart deployment/chat
kubectl rollout restart deployment/notification
kubectl rollout restart deployment/frontend

write-Host "Deployment completed successfully!" -ForegroundColor Green
write-Host "You can access the application at http://localhost" -ForegroundColor Cyan