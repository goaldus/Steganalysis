Get-ChildItem "C:\Users\omolnar\Desktop\pokus\" -Filter *.bmp | 
Foreach-Object {
    $filename = $_.Name
    $fullname = $_.FullName
    Start-Process java -ArgumentList '-jar', 'C:\Users\omolnar\Desktop\openstego-0.7.3\lib\openstego.jar embed -mf C:\Users\omolnar\Desktop\5kB.txt -cf C:\Users\omolnar\Desktop\pokus\2.bmp -sf "C:\Users\omolnar\Desktop\vystup\"$filename""' `
-RedirectStandardOutput '.\console.out' -RedirectStandardError '.\console.err'
}