Import-Module "c:\projects\neo4j\neo4j-community-3.3.0\bin\Neo4j-Management.psd1"

function update-password
{
  $user = "neo4j"
  $pass= "neo4j"
  $uri = "http://localhost:7474/user/neo4j/password"
  $json = "{
            ""password"" : ""bolt""
          }"
  
  $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $user,$pass)))

  Invoke-RestMethod -Uri $uri -Method Post -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ContentType "application/json; charset=UTF-8" -Body $json
}

Write-Host 'Installing Neo4jServer ...'
 
Install-Neo4jServer -Neo4jServer "c:\projects\neo4j\neo4j-community-3.3.0\" -PassThru | Start-Neo4jServer

Write-Host 'Neo4jServer installed.'

Start-Sleep -s 10

Write-Host 'Updating password ...'

update-password

Write-Host 'Password updated.'