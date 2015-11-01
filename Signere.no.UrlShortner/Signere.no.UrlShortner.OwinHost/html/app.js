var app=angular.module("urlshortner",[])

.controller('main',['$http',function($http){
  this.Title='Test';
  this.Url='';
  this.ShortUrl=undefined;
  this.AccessToken=undefined;
  this.Create=function(){
    $http.post('https://urlshortner.azurewebsites.net',{Url:this.Url}).then(function(result){
      console.log(result.data);
      this.ShortUrl=result.data.ShortUrl;
      this.AccessToken=result.data.AccessToken;
    })
  };
  
  this.UrlChange=function(){
    this.ShortUrl=undefined;
    this.AccessToken=undefined;
  }
}])

;