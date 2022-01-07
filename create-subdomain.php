<?php
$cpanel_user="whospost";
$cpanel_pass="Mohan@123";

// your cPanel skin
$cpanel_skin = 'paper_lantern';

// your cPanel domain
$cpanel_host = 'whospost.com';

// subdomain name
$subdomain = 'siva';

// directory - defaults to public_html/subdomain_name
$dir = 'public_html/mysubdomain';

// create the subdomain

$sock = fsockopen($cpanel_host,2082);
if(!$sock) {
  print('Socket error');
  exit();
}
//https://whospost.com:2083/cpsess5848408693/frontend/paper_lantern/subdomain/doadddomain.html?domain=arun&rootdomain=whospost.com&dir=arun.whospost.com&go=Create

$pass = base64_encode("$cpanel_user:$cpanel_pass");
//$in = "GET /frontend/paper_lantern/subdomain/doadddomain.html?rootdomain=$cpanel_host&domain=$subdomain&dir=$dir\r\n";
$in = "GET /frontend/paper_lantern/subdomain/doadddomain.html?domain=$subdomain&rootdomain=$cpanel_host&dir=$subdomain.$cpanel_host&go=Create\r\n";
$in .= "HTTP/1.0\r\n";
$in .= "Host:$cpanel_host\r\n";
$in .= "Authorization: Basic $pass\r\n";
$in .= "\r\n";

fputs($sock, $in);
while (!feof($sock)) {
  $result .= fgets ($sock,128);
}
fclose($sock);

echo $result;

?>