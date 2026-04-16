# clean cluster script 
Write-host "Cleaning cluster..."

$confirmation = Read-Host "Are you sure you want to clean the cluster? This action cannot be undone. (y/n)"
if ($confirmation -ne "y") {
    Write-Host "Cluster Cancelled" -foregroundcolor Yellow
    exit 0
}

write-host "n Deleting cluster resources..." -foregroundcolor Yellow

kubectl delete -f k8s/ingress.yaml --ignore-not-found
kubectl delete -f k8s/ingress-chat.yaml --ignore-not-found
kubectl delete -f k8s/ingress-notification.yaml --ignore-not-found
kubectl delete -f k8s/api.yaml --ignore-not-found
kubectl delete -f k8s/chat.yaml --ignore-not-found
kubectl delete -f k8s/notification.yaml --ignore-not-found
kubectl delete -f k8s/frontend.yaml --ignore-not-found
kubectl delete -f k8s/mongo.yaml --ignore-not-found
kubectl delete -f k8s/redis.yaml --ignore-not-found

write-host "Cluster cleaned successfully." -foregroundcolor Green


