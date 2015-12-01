var app=angular.module("urlshortner",[])

.controller('main',['$http',function($http){
  this.Title='Test';
  this.Url = '';
 this.Prefix = undefined;
  this.ShortUrl=undefined;
  this.AccessToken=undefined;
  this.Create=function(){
    var self=this;
      $http.post('https://s.signere.no', { Url: this.Url,Prefix:this.Prefix }).then(function(result) {
          console.log(result.data);
          self.ShortUrl = result.data.ShortUrl;
          self.AccessToken = result.data.AccessToken;
      });
  };
  
  this.UrlChange=function(){
    this.ShortUrl=undefined;
    this.AccessToken=undefined;
  }
}])

;