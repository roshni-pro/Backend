using AngularJSAuthentication.API.DataContract;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Managers
{
    public class PagePermissionManager
    {
        public async Task<List<PeoplePageDc>> GetPeoplePagePermissionAsync(int peopleId, List<string> roleIds)
        {
            List<PeoplePageDc> PeoplePageDc = new List<PeoplePageDc>();

            if (roleIds != null && roleIds.Any())
            {
                using (var authContext = new AuthContext())
                {
                    var peoplepagebuttons = await authContext.PeoplePageAccessPermission.Where(x => x.PeopleId == peopleId && x.IsActive && !x.IsDeleted).Select(x => new { x.PeopleId, x.PageButton.ButtonMasterId, x.PageButton.PageMasterId }).Distinct().ToListAsync();
                    if (!authContext.OverrideRolePagePermission.Any(x => x.PeopleId == peopleId))
                    {
                        PeoplePageDc = await authContext.RolePagePermission.Where(x => roleIds.Contains(x.RoleId) && !x.PageMaster.ParentId.HasValue && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,
                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId
                            }).ToList(),
                        }).ToListAsync();
                        PeoplePageDc = PeoplePageDc.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                        PeoplePageDc = PeoplePageDc.OrderBy(x => x.Sequence).ToList();
                        var pageids = PeoplePageDc.Select(x => x.Id).Distinct().ToList();

                        var AllChildPageDcs = await authContext.RolePagePermission.Where(x => roleIds.Contains(x.RoleId) && x.PageMaster.ParentId.HasValue && pageids.Contains(x.PageMaster.ParentId.Value) && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,
                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId
                            }).ToList()
                        }).ToListAsync();

                        foreach (var item in PeoplePageDc)
                        {
                            foreach (var button in item.PeoplePageButtonDcs)
                            {
                                button.Active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == item.Id) : false;
                            }


                            item.ChildPageDcs = AllChildPageDcs.Where(x => x.ParentId == item.Id).ToList();

                            //await authContext.RolePagePermission.Where(x => roleIds.Contains(x.RoleId) && x.PageMaster.ParentId == item.Id && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                            //{
                            //    ClassName = x.PageMaster.ClassName,
                            //    Id = x.PageMaster.Id,
                            //    PageName = x.PageMaster.PageName,
                            //    RouteName = x.PageMaster.RouteName,
                            //    ParentId = x.PageMaster.ParentId,
                            //    Sequence = x.PageMaster.Sequence,
                            //    IconClassName = x.PageMaster.IconClassName,
                            //    IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            //    PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            //    {
                            //        ButtonClassName = y.ButtonMaster.ButtonClassName,
                            //        ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                            //        ButtonName = y.ButtonMaster.ButtonName,
                            //        Id = y.ButtonMaster.Id,
                            //        PageId = y.PageMasterId
                            //    }).ToList()
                            //}).ToListAsync();
                            foreach (var child in item.ChildPageDcs.ToList())
                            {
                                foreach (var button in child.PeoplePageButtonDcs)
                                {
                                    button.Active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == child.Id) : false;
                                }
                            }
                            item.ChildPageDcs = item.ChildPageDcs.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                            item.ChildPageDcs = item.ChildPageDcs.OrderBy(x => x.Sequence).ToList();
                        }
                    }
                    else
                    {
                        PeoplePageDc = await authContext.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId && !x.PageMaster.ParentId.HasValue && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,
                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId,
                                Active = false
                            }).ToList()
                        }).ToListAsync();
                        PeoplePageDc = PeoplePageDc.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                        PeoplePageDc = PeoplePageDc.OrderBy(x => x.Sequence).ToList();
                        var pageids = PeoplePageDc.Select(x => x.Id).Distinct().ToList();
                        var AllChildPageDcs = await authContext.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId && x.PageMaster.ParentId.HasValue && pageids.Contains(x.PageMaster.ParentId.Value) && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,
                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId,
                                Active = false
                            }).ToList()
                        }).ToListAsync();
                        foreach (var item in PeoplePageDc)
                        {
                            foreach (var button in item.PeoplePageButtonDcs.ToList())
                            {
                                button.Active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == item.Id) : false;
                            }


                            item.ChildPageDcs = AllChildPageDcs.Where(x => x.ParentId == item.Id).ToList();                                                      

                            //await authContext.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId && x.PageMaster.ParentId == item.Id && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                            //{
                            //    ClassName = x.PageMaster.ClassName,
                            //    Id = x.PageMaster.Id,
                            //    PageName = x.PageMaster.PageName,
                            //    RouteName = x.PageMaster.RouteName,
                            //    ParentId = x.PageMaster.ParentId,
                            //    Sequence = x.PageMaster.Sequence,
                            //    IconClassName = x.PageMaster.IconClassName,
                            //    IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            //    PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            //    {
                            //        ButtonClassName = y.ButtonMaster.ButtonClassName,
                            //        ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                            //        ButtonName = y.ButtonMaster.ButtonName,
                            //        Id = y.ButtonMaster.Id,
                            //        PageId = y.PageMasterId,
                            //        Active = false
                            //    }).ToList()
                            //}).ToListAsync();

                            item.ChildPageDcs.ForEach(child =>
                            {
                                child.PeoplePageButtonDcs.ForEach(button =>
                                {
                                    bool active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == child.Id) : false;
                                    button.Active = active;
                                });

                            });
                            //foreach (var child in item.ChildPageDcs.ToList())
                            //{
                            //    foreach (var button in child.PeoplePageButtonDcs.ToList())
                            //    {
                            //        bool active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == item.Id) : false;
                            //        button.Active = active;
                            //    }
                            //}
                            item.ChildPageDcs = item.ChildPageDcs.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                            item.ChildPageDcs = item.ChildPageDcs.OrderBy(x => x.Sequence).ToList();
                        }
                    }
                }
            }
            return PeoplePageDc;
        }

        internal List<PeoplePageDc> GetPeoplePage(int PeopleId, List<string> lstRoleids)
        {
#if DEBUG

            return GetPeoplePagePermission(PeopleId, lstRoleids);
#else
                Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                var result = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.PeoplePageKey(PeopleId), () => GetPeoplePagePermission(PeopleId, lstRoleids));
                return result;
#endif
        }

        public List<PeoplePageDc> GetPeoplePagePermission(int peopleId, List<string> roleIds)
        {
            List<PeoplePageDc> PeoplePageDc = new List<PeoplePageDc>();

            if (roleIds != null && roleIds.Any())
            {
                using (var authContext = new AuthContext())
                {
                    var peoplepagebuttons = authContext.PeoplePageAccessPermission.Where(x => x.PeopleId == peopleId && x.IsActive && !x.IsDeleted).Select(x => new { x.PeopleId, x.PageButton.ButtonMasterId, x.PageButton.PageMasterId }).Distinct().ToList();
                    if (!authContext.OverrideRolePagePermission.Any(x => x.PeopleId == peopleId))
                    {
                        PeoplePageDc = authContext.RolePagePermission.Where(x => roleIds.Contains(x.RoleId) && !x.PageMaster.ParentId.HasValue && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,

                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId
                            }).ToList(),
                        }).ToList();
                        PeoplePageDc = PeoplePageDc.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                        PeoplePageDc = PeoplePageDc.OrderBy(x => x.Sequence).ToList();
                        var pageids = PeoplePageDc.Select(x => x.Id).Distinct().ToList();

                        var AllChildPageDcs = authContext.RolePagePermission.Where(x => roleIds.Contains(x.RoleId) && x.PageMaster.ParentId.HasValue && pageids.Contains(x.PageMaster.ParentId.Value) && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,

                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId
                            }).ToList()
                        }).ToList();

                        foreach (var item in PeoplePageDc)
                        {
                            foreach (var button in item.PeoplePageButtonDcs)
                            {
                                button.Active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == item.Id) : false;
                            }


                            item.ChildPageDcs = AllChildPageDcs.Where(x => x.ParentId == item.Id).ToList();

                            //await authContext.RolePagePermission.Where(x => roleIds.Contains(x.RoleId) && x.PageMaster.ParentId == item.Id && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                            //{
                            //    ClassName = x.PageMaster.ClassName,
                            //    Id = x.PageMaster.Id,
                            //    PageName = x.PageMaster.PageName,
                            //    RouteName = x.PageMaster.RouteName,
                            //    ParentId = x.PageMaster.ParentId,
                            //    Sequence = x.PageMaster.Sequence,
                            //    IconClassName = x.PageMaster.IconClassName,
                            //    IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            //    PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            //    {
                            //        ButtonClassName = y.ButtonMaster.ButtonClassName,
                            //        ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                            //        ButtonName = y.ButtonMaster.ButtonName,
                            //        Id = y.ButtonMaster.Id,
                            //        PageId = y.PageMasterId
                            //    }).ToList()
                            //}).ToListAsync();
                            foreach (var child in item.ChildPageDcs.ToList())
                            {
                                foreach (var button in child.PeoplePageButtonDcs)
                                {
                                    button.Active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == child.Id) : false;
                                }
                            }
                            item.ChildPageDcs = item.ChildPageDcs.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                            item.ChildPageDcs = item.ChildPageDcs.OrderBy(x => x.Sequence).ToList();
                        }
                    }
                    else
                    {
                        PeoplePageDc =  authContext.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId && !x.PageMaster.ParentId.HasValue && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,

                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId,
                                Active = false
                            }).ToList()
                        }).ToList();
                        PeoplePageDc = PeoplePageDc.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                        PeoplePageDc = PeoplePageDc.OrderBy(x => x.Sequence).ToList();
                        var pageids = PeoplePageDc.Select(x => x.Id).Distinct().ToList();
                        var AllChildPageDcs = authContext.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId && x.PageMaster.ParentId.HasValue && pageids.Contains(x.PageMaster.ParentId.Value) && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                        {
                            ClassName = x.PageMaster.ClassName,
                            Id = x.PageMaster.Id,
                            PageName = x.PageMaster.PageName,
                            RouteName = x.PageMaster.RouteName,
                            ParentId = x.PageMaster.ParentId,
                            Sequence = x.PageMaster.Sequence,
                            IconClassName = x.PageMaster.IconClassName,
                            IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            IsGroup2PortalUrl = x.PageMaster.IsGroup2PortalUrl,

                            PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            {
                                ButtonClassName = y.ButtonMaster.ButtonClassName,
                                ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                                ButtonName = y.ButtonMaster.ButtonName,
                                Id = y.ButtonMaster.Id,
                                PageId = y.PageMasterId,
                                Active = false
                            }).ToList()
                        }).ToList();
                        foreach (var item in PeoplePageDc)
                        {
                            foreach (var button in item.PeoplePageButtonDcs.ToList())
                            {
                                button.Active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == item.Id) : false;
                            }


                            item.ChildPageDcs = AllChildPageDcs.Where(x => x.ParentId == item.Id).ToList();

                            //await authContext.OverrideRolePagePermission.Where(x => x.PeopleId == peopleId && x.PageMaster.ParentId == item.Id && x.PageMaster.IsActive && !x.PageMaster.IsDeleted && x.IsActive && !x.IsDeleted).Select(x => new PeoplePageDc
                            //{
                            //    ClassName = x.PageMaster.ClassName,
                            //    Id = x.PageMaster.Id,
                            //    PageName = x.PageMaster.PageName,
                            //    RouteName = x.PageMaster.RouteName,
                            //    ParentId = x.PageMaster.ParentId,
                            //    Sequence = x.PageMaster.Sequence,
                            //    IconClassName = x.PageMaster.IconClassName,
                            //    IsNewPortalUrl = x.PageMaster.IsNewPortalUrl,
                            //    PeoplePageButtonDcs = x.PageMaster.PageButtons.Where(y => y.IsActive && !y.IsDeleted).Select(y => new PeoplePageButtonDc
                            //    {
                            //        ButtonClassName = y.ButtonMaster.ButtonClassName,
                            //        ButtonHtmlId = y.ButtonMaster.ButtonHtmlId,
                            //        ButtonName = y.ButtonMaster.ButtonName,
                            //        Id = y.ButtonMaster.Id,
                            //        PageId = y.PageMasterId,
                            //        Active = false
                            //    }).ToList()
                            //}).ToListAsync();

                            item.ChildPageDcs.ForEach(child =>
                            {
                                child.PeoplePageButtonDcs.ForEach(button =>
                                {
                                    bool active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == child.Id) : false;
                                    button.Active = active;
                                });

                            });
                            //foreach (var child in item.ChildPageDcs.ToList())
                            //{
                            //    foreach (var button in child.PeoplePageButtonDcs.ToList())
                            //    {
                            //        bool active = peoplepagebuttons != null && peoplepagebuttons.Any() ? peoplepagebuttons.Any(x => x.ButtonMasterId == button.Id && x.PageMasterId == item.Id) : false;
                            //        button.Active = active;
                            //    }
                            //}
                            item.ChildPageDcs = item.ChildPageDcs.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                            item.ChildPageDcs = item.ChildPageDcs.OrderBy(x => x.Sequence).ToList();
                        }
                    }
                }
            }
            return PeoplePageDc;
        }


    }
}