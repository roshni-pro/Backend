using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ElasticLanguageSearch;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class LanguageConvertHelper
    {
        public List<ElasticLanguageData> GetConvertLanguageData(List<ElasticLanguageDataRequest> elasticLanguageDatas, string lang)
        {
            List<ElasticLanguageData> ElasticLanguageDatas = new List<ElasticLanguageData>();
            ElasticLanguageHelper elasticLanguageHelper = new ElasticLanguageHelper();
            string ElasticLngIndex = "elasticlanguage-" + lang;
            elasticLanguageHelper.CreateLanguageIndex(ElasticLngIndex);
            // string.Join("+", itemnumbers);
            string query = "{   \"query\": {     \"bool\": {       \"must\": [  { \"query_string\": {  \"query\": \"{#langData#}\", \"fields\": [ \"englishtext\" ]   }          }      ]     }   },   \"from\": {#from#},   \"size\": {#size#} }";
            query = query.Replace("{#langData#}", string.Join(" OR ", elasticLanguageDatas.Distinct().Where(x => !string.IsNullOrEmpty(x.englishtext)).Select(x => "(" + x.englishtext + ")")));
            query = query.Replace("{#from#}", "0");
            query = query.Replace("{#size#}", "10000");
            ElasticLanguageDatas = elasticLanguageHelper.GetAllElasticLanguageQuery(query, ElasticLngIndex);
            Annotate annotate = new Annotate();
            if (ElasticLanguageDatas.Any())
            {
                var newLanguageData = elasticLanguageDatas.Where(x => !string.IsNullOrEmpty(x.englishtext) && !ElasticLanguageDatas.Select(y => y.englishtext).Contains(x.englishtext)).ToList();

                if (newLanguageData.Any())
                {
                    ConcurrentBag<ElasticLanguageData> translitratetextlst = new ConcurrentBag<ElasticLanguageData>();
                    List<ElasticLanguageData> elsDatas = new List<ElasticLanguageData>();
                    List<ElasticLanguageData> elsTranslateDatas = new List<ElasticLanguageData>();
                    ParallelLoopResult parellegooglelResult = Parallel.ForEach(newLanguageData.Where(x => x.IsTranslate), (x) =>
                      {
                          x.converttext = AsyncContext.Run(() => annotate.GetgoogleTranslate(x.englishtext, "en", lang));
                          char[] strArray = x.converttext.ToCharArray();
                          int totalSpecialChar = 0;
                          foreach (var item in strArray)
                          {
                              if (item == '@' || item =='_' )
                                  totalSpecialChar++;
                          }
                          string strstatement = x.converttext.ToString();
                          while (totalSpecialChar != 0)
                          {
                              if (strstatement.Contains("@"))
                              {
                                  string word = strstatement.Substring(strstatement.IndexOf("@") + 1);
                                  strstatement = word.Substring(word.IndexOf(" "));
                                  word = word.Substring(0, word.IndexOf(" ") - 1);
                                  ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                                  elasticLanguageData.englishtext = "@" + word ;
                                  elasticLanguageData.converttext = word;
                                  translitratetextlst.Add(elasticLanguageData);
                              }
                              if (strstatement.Contains("_"))
                              {
                                  string word = strstatement.Substring(strstatement.IndexOf("_") - 1);
                                  strstatement = word.Substring(word.IndexOf(" "));
                                  word = word.Substring(0, word.IndexOf(" "));
                                  ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                                  elasticLanguageData.englishtext = word;
                                  elasticLanguageData.converttext = word.Replace("_", "");
                                  translitratetextlst.Add(elasticLanguageData);
                              }
                              totalSpecialChar--;
                          }
                          elsTranslateDatas.Add(new ElasticLanguageData { converttext = x.converttext, englishtext = x.englishtext });
                      });


                    if (parellegooglelResult.IsCompleted)
                    {
                        elsDatas.AddRange( newLanguageData.Where(x => !x.IsTranslate).Select(x => new ElasticLanguageData { converttext = x.converttext, englishtext = x.englishtext }).ToList());
                        //elsDatas.AddRange(translitratetextlst);

                        ParallelLoopResult parellelResult = Parallel.ForEach(elsDatas, (x) =>
                        {
                            x.converttext = AsyncContext.Run(() => annotate.GetTranslatedText(x.englishtext, lang));
                        });
                        ParallelLoopResult parellelResult1 = Parallel.ForEach(translitratetextlst, (x) =>
                        {
                            x.converttext = AsyncContext.Run(() => annotate.GetTranslatedText(x.converttext, lang));
                        });
                       
                        if (parellelResult.IsCompleted && parellelResult1.IsCompleted)
                        {
                            elsDatas.AddRange(elsTranslateDatas);
                            if (translitratetextlst.Any())
                            {
                                foreach (var item in elsDatas)
                                {
                                    foreach (var txt in translitratetextlst)
                                    {
                                        item.converttext = item.converttext.Replace(txt.englishtext, txt.converttext);
                                    }
                                }
                            }

                            if (elasticLanguageHelper.AddElasticLanguageData(ElasticLngIndex, elsDatas.ToList()))
                            {
                                ElasticLanguageDatas.AddRange(elsDatas);
                            }
                        }
                    }
                }
            }
            else
            {
                ConcurrentBag<ElasticLanguageData> translitratetextlst = new ConcurrentBag<ElasticLanguageData>();
                List<ElasticLanguageData> elsDatas = new List<ElasticLanguageData>();
                List<ElasticLanguageData> elsTranslateDatas = new List<ElasticLanguageData>();
                ParallelLoopResult parellegooglelResult = Parallel.ForEach(elasticLanguageDatas.Where(x => x.IsTranslate), (x) =>
                  {
                      x.converttext = AsyncContext.Run(() => annotate.GetgoogleTranslate(x.englishtext, "en", lang));
                      char[] strArray = x.converttext.ToCharArray();
                      int totalSpecialChar = 0;
                      foreach (var item in strArray)
                      {
                          if (item == '@' || item == '_')
                              totalSpecialChar++;
                      }
                      string strstatement = x.converttext.ToString();
                      while (totalSpecialChar != 0)
                      {
                          if (strstatement.Contains("@"))
                          {
                              string word = strstatement.Substring(strstatement.IndexOf("@") + 1);
                              strstatement = word.Substring(word.IndexOf(" "));
                              word = word.Substring(0, word.IndexOf(" ") - 1);
                              ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                              elasticLanguageData.englishtext = "@" + word;
                              elasticLanguageData.converttext = word;
                              translitratetextlst.Add(elasticLanguageData);
                          }
                          if (strstatement.Contains("_"))
                          {
                              string word = strstatement.Substring(strstatement.IndexOf("_") - 1);
                              strstatement = word.Substring(word.IndexOf(" "));
                              word = word.Substring(0, word.IndexOf(" "));
                              ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                              elasticLanguageData.englishtext = word;
                              elasticLanguageData.converttext = word.Replace("_", "");
                              translitratetextlst.Add(elasticLanguageData);
                          }
                          totalSpecialChar--;
                      }
                      elsTranslateDatas.Add(new ElasticLanguageData { converttext = x.converttext, englishtext = x.englishtext });
                  });


                if (parellegooglelResult.IsCompleted)
                {
                    elsDatas.AddRange(elasticLanguageDatas.Where(x => !x.IsTranslate).Select(x => new ElasticLanguageData { converttext = x.converttext, englishtext = x.englishtext }).ToList());
                    //elsDatas.AddRange(translitratetextlst);
                    ParallelLoopResult parellelResult = Parallel.ForEach(elsDatas, (x) =>
                    {
                        x.converttext = AsyncContext.Run(() => annotate.GetTranslatedText(x.englishtext, lang));
                    });

                    ParallelLoopResult parellelResult1 = Parallel.ForEach(translitratetextlst, (x) =>
                    {
                        x.converttext = AsyncContext.Run(() => annotate.GetTranslatedText(x.converttext, lang));
                    });

                    if (parellelResult.IsCompleted && parellelResult1.IsCompleted)
                    {
                        elsDatas.AddRange(elsTranslateDatas);
                        if (translitratetextlst.Any())
                        {
                            foreach (var item in elsDatas)
                            {
                                foreach (var txt in translitratetextlst)
                                {
                                    item.converttext = item.converttext.Replace(txt.englishtext, txt.converttext);
                                }
                            }

                        }

                        if (elasticLanguageHelper.AddElasticLanguageData(ElasticLngIndex, elsDatas.ToList()))
                        {
                            ElasticLanguageDatas.AddRange(elsDatas);
                        }
                    }
                }
            }
            return ElasticLanguageDatas;
        }

        public bool CheckConvertLanguageData(List<ElasticLanguageData> elasticLanguageDatas, string lang)
        {
            List<ElasticLanguageData> ElasticLanguageDatas = new List<ElasticLanguageData>();
            ElasticLanguageHelper elasticLanguageHelper = new ElasticLanguageHelper();
            string ElasticLngIndex = "elasticlanguage-" + lang;
            elasticLanguageHelper.CreateLanguageIndex(ElasticLngIndex);
            // string.Join("+", itemnumbers);
            string query = "{   \"query\": {     \"bool\": {       \"must\": [  { \"query_string\": {  \"query\": \"{#langData#}\", \"fields\": [ \"englishtext\" ]   }          }      ]     }   },   \"from\": {#from#},   \"size\": {#size#} }";
            query = query.Replace("{#langData#}", string.Join(" OR ", elasticLanguageDatas.Where(x => !string.IsNullOrEmpty(x.englishtext)).Select(x => "(" + x.englishtext + ")")));
            //query = query.Replace("{#from#}", "0");
            //query = query.Replace("{#size#}", elasticLanguageDatas.Count.ToString());
            ElasticLanguageDatas = elasticLanguageHelper.GetAllElasticLanguageQuery(query, ElasticLngIndex);
            Annotate annotate = new Annotate();
            if (ElasticLanguageDatas.Any())
            {
                var newLanguageData = elasticLanguageDatas.Where(x => !string.IsNullOrEmpty(x.englishtext) && !ElasticLanguageDatas.Select(y => y.englishtext).Contains(x.englishtext)).ToList();

                if (newLanguageData.Any())
                {
                    ConcurrentBag<ElasticLanguageData> translitratetextlst = new ConcurrentBag<ElasticLanguageData>();
                    ParallelLoopResult parellegooglelResult = Parallel.ForEach(newLanguageData, (x) =>
                    {
                        x.converttext = AsyncContext.Run(() => annotate.GetgoogleTranslate(x.englishtext, "en", lang));
                        if (x.converttext.Contains("@"))
                        {
                            string word = x.converttext.Substring(x.converttext.IndexOf("@") + 1);
                            word = word.Substring(x.converttext.IndexOf("@") - 1);
                            ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                            elasticLanguageData.englishtext = "@" + word + "@";
                            elasticLanguageData.converttext = word;
                            translitratetextlst.Add(elasticLanguageData);
                        }
                        if (x.converttext.Contains("_"))
                        {
                            string word = x.converttext.Substring(x.converttext.IndexOf("_") - 2);
                            word = word.Substring(x.converttext.IndexOf(" ") - 1);
                            ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                            elasticLanguageData.englishtext = word;
                            elasticLanguageData.converttext = word.Replace("_", "");
                            translitratetextlst.Add(elasticLanguageData);
                        }
                    });


                    if (parellegooglelResult.IsCompleted && translitratetextlst.Any())
                    {

                        ParallelLoopResult parellelResult = Parallel.ForEach(translitratetextlst, (x) =>
                        {
                            x.converttext = AsyncContext.Run(() => annotate.GetTranslatedText(x.converttext, lang));
                        });

                        if (parellelResult.IsCompleted)
                        {
                            if (translitratetextlst.Any())
                            {
                                foreach (var item in newLanguageData)
                                {
                                    foreach (var txt in translitratetextlst)
                                    {
                                        item.converttext = item.converttext.Replace(txt.englishtext, txt.converttext);
                                    }
                                }

                            }

                            if (elasticLanguageHelper.AddElasticLanguageData(ElasticLngIndex, newLanguageData))
                            {
                                ElasticLanguageDatas.AddRange(newLanguageData);
                            }
                        }
                    }

                }
            }
            else
            {

                ConcurrentBag<ElasticLanguageData> translitratetextlst = new ConcurrentBag<ElasticLanguageData>();
                ParallelLoopResult parellegooglelResult = Parallel.ForEach(elasticLanguageDatas, (x) =>
                {
                    x.converttext = AsyncContext.Run(() => annotate.GetgoogleTranslate(x.englishtext, "en", lang));
                    if (x.converttext.Contains("@"))
                    {
                        string word = x.converttext.Substring(x.converttext.IndexOf("@") + 1);
                        word = word.Substring(x.converttext.IndexOf("@") - 1);
                        ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                        elasticLanguageData.englishtext = "@" + word + "@";
                        elasticLanguageData.converttext = word;
                        translitratetextlst.Add(elasticLanguageData);
                    }
                    if (x.converttext.Contains("_"))
                    {
                        string word = x.converttext.Substring(x.converttext.IndexOf("_") - 2);
                        word = word.Substring(x.converttext.IndexOf(" ") - 1);
                        ElasticLanguageData elasticLanguageData = new ElasticLanguageData();
                        elasticLanguageData.englishtext = word;
                        elasticLanguageData.converttext = word.Replace("_", "");
                        translitratetextlst.Add(elasticLanguageData);
                    }

                });


                if (parellegooglelResult.IsCompleted)
                {
                    ParallelLoopResult parellelResult = Parallel.ForEach(translitratetextlst, (x) =>
                    {
                        x.converttext = AsyncContext.Run(() => annotate.GetTranslatedText(x.converttext, lang));
                    });

                    if (parellelResult.IsCompleted)
                    {
                        if (translitratetextlst.Any())
                        {
                            foreach (var item in elasticLanguageDatas)
                            {
                                foreach (var txt in translitratetextlst)
                                {
                                    item.converttext = item.converttext.Replace(txt.englishtext, txt.converttext);
                                }
                            }

                        }

                        if (elasticLanguageHelper.AddElasticLanguageData(ElasticLngIndex, elasticLanguageDatas))
                        {
                            ElasticLanguageDatas.AddRange(elasticLanguageDatas);
                        }
                    }
                }

            }
            return true;
        }
    }
}