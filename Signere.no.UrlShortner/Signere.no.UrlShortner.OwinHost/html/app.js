var app=angular.module("urlshortner",[])

.controller('main',['$http',function($http){
  this.Title='Test';
  this.Url='';
  this.ShortUrl=undefined;
  this.AccessToken=undefined;
  this.Create=function(){
    var self=this;
    $http.post('https://urlshortner.azurewebsites.net',{Url:this.Url}).then(function(result){
      console.log(result.data);
      self.ShortUrl=result.data.ShortUrl;
      self.AccessToken=result.data.AccessToken;
    })
  };
  
  this.UrlChange=function(){
    this.ShortUrl=undefined;
    this.AccessToken=undefined;
  }
}])

;