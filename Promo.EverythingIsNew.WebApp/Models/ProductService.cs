using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AltLanDS.Beeline.DpcProxy.Client;
using AltLanDS.Beeline.DpcProxy.Client.Domain;
using AutoMapper;

using System.ComponentModel;
using System.Runtime.Serialization;

using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.ComponentModel.DataAnnotations;


namespace Promo.EverythingIsNew.WebApp.Models
{
    public class ProductService 
    {
        private static string _siteUrlFormat = ConfigurationManager.AppSettings["altlands:dpc:site-url"];

        public DpcProxyDbContext Db;
        public ProductService(DpcProxyDbContext db)
        {
            Db = db;
        }

        public TarifInfo MapTariff(MobileTariff dpcTariff, string marketCode, ProductParseType parseType = ProductParseType.Mobapp)
        {
            try
            {
                var rez = new List<TarifViewModel>();

                var productInfo = XmlDeserialize.XmlDeserializeFromString<AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductInfo>(dpcTariff.Data);
                foreach (var pp in productInfo.Products)
                {
                    foreach (var webEntity in pp.ProductWebEntities)
                    {
                        if (webEntity.WebEntity == null || webEntity.WebEntity.SOC.ToLower() != dpcTariff.SocName.ToLower())
                            continue;

                        var model = new TarifViewModel();

                        FillProduct(pp, webEntity, model, parseType);


                        //var iconParameters = Mapper.Map<List<IconParameter>>(pp.Parameters.Where(p => (p.Modifiers.Any(m => m.Id == 2208))).ToList()) //Главный в карточке
                        //    .Where(p => !string.IsNullOrEmpty(p.Type)).ToList();

                        var iconParameters = new List<IconParameter>();


                        var filledBaseParameters = pp.Parameters.Where(p => p.BaseParameter != null && p.BaseParameter.Id != null);
                        foreach (var filledBaseParameter in filledBaseParameters)
                        {
                            decimal value = 0;
                            Decimal.TryParse(filledBaseParameter.Value, out value);
                            if (value == 0)
                                value = filledBaseParameter.NumValue ?? 0;

                            var valuePeriodMobile = (filledBaseParameter.Unit != null ? filledBaseParameter.Unit.ShortDisplay : "");
                            var sortOrder = filledBaseParameter.SortOrder ?? 0;

                            //if (value == 0)
                            //{
                            //    value = 0;
                            //}

                            switch (filledBaseParameter.BaseParameter.Id)
                            {
                                case 2242: //Абонентская плата 
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Cost, Type = "Абонентская плата", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2705://Пакет минут
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Communication, Type = "Пакет минут", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2706://Пакет интернета
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Internet, Type = "Пакет интернета", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2707://Пакет SMS
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Messages, Type = "Пакет SMS", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2246://Пакет SMS
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.ServiceConnection, Type = "Стоимость подключения (цена)", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;

                            }
                        }


                        model.IconParameters = iconParameters;

                        //if (pp.Parameters.Any(p => p.BaseParameter != null && p.BaseParameter.Id == 2242)) //Абонентская плата
                        //{
                        //    model.SummaryParameters = Mapper.Map<List<Parameter>>(pp.Parameters
                        //        .Where(p => p.Modifiers.Any(m => m.Id == 2208)) //Главный в карточке
                        //        .Where(p => p.BaseParameter != null && p.BaseParameter.Id != 2242)  //Абонентская плата
                        //        .Where(p => p.Group != null && p.Group.Id != 2203))
                        //        .OrderBy(p => p.SortOrder)
                        //        .ToList();
                        //}
                        model.SiteUrl = string.Format(_siteUrlFormat, "tariff", marketCode, webEntity.WebEntity.SOC, dpcTariff.IsB2B ? "/B2B" : "");
                        if (model.Name == null)
                            model.Name = pp.ArchiveTitle;


                        //var relevantServices = pp.ServicesOnTariff.Where(s => s.Parent.Modifiers.Any(m => m.Alias == "Recommend")).Select(s => s.Service.Id);
                        //model.RelevantServices = dpcTariff.MobileServices.ToList().Where(s => relevantServices.Contains(s.ProductId)).Select(s => s.SocName).ToList();

                        rez.Add(model);
                    }
                }

                return new TarifInfo
                {
                    Soc = dpcTariff.SocName,
                    Info = rez.OrderBy(o => o.Order)
                };
            }
            catch (Exception e)
            {
                //ApiBackendEvents.Log.MappingProductException(dpcTariff.ProductId, e);
                return null;
            }
        }

        public ServicesInfo MapService(MobileService dpcService, string marketCode, ProductParseType parseType = ProductParseType.Mobapp)
        {
            try
            {
                var rez = new List<ServiceViewModel>();

                var productInfo = XmlDeserialize.XmlDeserializeFromString<AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductInfo>(dpcService.Data);
                foreach (var serv in productInfo.Products)
                {
                    foreach (var webEntity in serv.ProductWebEntities)
                    {
                        if (webEntity.WebEntity == null || webEntity.WebEntity.SOC.ToLower() != dpcService.SocName.ToLower())
                            continue;

                        var model = new ServiceViewModel();

                        FillProduct(serv, webEntity, model, parseType);
                        //IconParameterType iconParameterType = IconParameterType.Unknown;
                        //var iconParameter = serv.Parameters.FirstOrDefault(p => (p.Modifiers.Any(m => m.Id == 2205))); //Показывать в плитке

                        //if (iconParameter == null)
                        //{
                        //    var showSubscriptionFee = serv.Modifiers.Any(m => m.Id == 2232); //Показывать абонентскую плату в плитке
                        //    if (showSubscriptionFee)
                        //    {
                        //        iconParameter = serv.Parameters.FirstOrDefault(p => p.BaseParameter != null && (p.BaseParameter.Id == 2242)); //Абонентская плата
                        //        iconParameterType = IconParameterType.Cost;
                        //    }
                        //    else
                        //    {
                        //        iconParameter = serv.Parameters.FirstOrDefault(p => p.BaseParameter != null && (p.BaseParameter.Id == 2246)); //стоимость подключения
                        //        iconParameterType = IconParameterType.ServiceConnection;
                        //    }
                        //}

                        //if (iconParameter != null)
                        //{
                        //    var newParam = Mapper.Map<IconParameter>(iconParameter);
                        //    newParam.Type = iconParameter.BaseParameter == null ? null : iconParameter.BaseParameter.Title;

                        //    if (iconParameterType == IconParameterType.Unknown)
                        //    {
                        //        iconParameterType = EnumHelper.GetValueFromDescription<IconParameterType>(newParam.Type);
                        //    }

                        //    newParam.TypeId = iconParameterType;
                        //    model.IconParameters.Add(newParam);
                        //}

                        model.SiteUrl = string.Format(_siteUrlFormat, "service", marketCode, webEntity.WebEntity.SOC, dpcService.IsB2B ? "/B2B" : "");
                        if (model.Name == null)
                            model.Name = serv.ArchiveTitle;

                        model.Description = HtmlParse(serv.MarketingProduct == null ? serv.Description : serv.MarketingProduct.Description, serv.Id, parseType);

                        //var relevantTarifs = serv.TariffsOnService.Where(s => s.Parent.Modifiers.Any(m => m.Alias == "Recommend")).Select(s => s.Tariff.Id);
                        //model.RelevantTarifs = dpcService.MobileTariffs.ToList().Where(s => relevantTarifs.Contains(s.ProductId)).Select(s => s.SocName).ToList();

                        var iconParameters = new List<IconParameter>();


                        var filledBaseParameters = serv.Parameters.Where(p => p.BaseParameter != null && p.BaseParameter.Id != null);
                        foreach (var filledBaseParameter in filledBaseParameters)
                        {
                            decimal value = 0;
                            Decimal.TryParse(filledBaseParameter.Value, out value);
                            var valuePeriodMobile = (filledBaseParameter.Unit != null ? filledBaseParameter.Unit.ShortDisplay : "");
                            var sortOrder = filledBaseParameter.SortOrder ?? 0;

                            //if (value == 0)
                            //{
                            //    value = 0;
                            //}

                            switch (filledBaseParameter.BaseParameter.Id)
                            {
                                case 2242: //Абонентская плата 
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Cost, Type = "Абонентская плата", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2705://Пакет минут
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Communication, Type = "Пакет минут", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2706://Пакет интернета
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Internet, Type = "Пакет интернета", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2707://Пакет SMS
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.Messages, Type = "Пакет SMS", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;
                                case 2246://Пакет SMS
                                    iconParameters.Add(new IconParameter() { Id = filledBaseParameter.Id, TypeId = IconParameterType.ServiceConnection, Type = "Стоимость подключения (цена)", SortOrder = sortOrder, Value = value, ValuePeriodMobile = valuePeriodMobile });
                                    break;

                            }
                        }


                        model.IconParameters = iconParameters;

                        rez.Add(model);
                    }
                }

                return new ServicesInfo
                {
                    Soc = dpcService.SocName,
                    Info = rez.OrderBy(o => o.Order)
                };
            }
            catch (Exception e)
            {
                //ApiBackendEvents.Log.MappingProductException(dpcService.ProductId, e);
                return null;
            }
        }

        public void FillProduct(AltLanDS.Beeline.DpcProxy.Client.Dpc.Product dpcProduct, AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductWebEntity entity, ProductViewModel model, ProductParseType parseType)
        {

            model.SiteProductId = dpcProduct.Id;

            if (dpcProduct.MarketingProduct != null)
            {
                model.Name = dpcProduct.MarketingProduct.Title;
                if (dpcProduct.Modifiers.Any(m => m.Id == 2169))
                    model.isArchive = true;
            }
            else
            {
                model.Name = entity.WebEntity.Title ?? dpcProduct.Title;
                model.isArchive = true;
            }

            model.Soc = entity.WebEntity.SOC;
            model.NoAuthVisible = dpcProduct.Modifiers.All(m => m.Alias != "HideForUnauthorized");

            model.Categories = dpcProduct.MarketingProduct == null ? new List<Category>() :
                dpcProduct.MarketingProduct.Categories.Select(cat => new Category()
                {
                    Id = cat.Id,
                    Title = cat.Title,
                    SortOrder = cat.SortOrder
                }).ToList();

            if (model.Categories.Any())
                model.Categories = new List<Category> { model.Categories.FirstOrDefault() }; //Если продукт относится к нескольким категориям, оставляем одну первую по SortOrder


            if (dpcProduct.MarketingProduct != null)
            {
                var imageSmall = dpcProduct.MarketingProduct.Images.FirstOrDefault(mp => mp.Type.Id == 2713);
                model.ImageSmall = imageSmall == null ? null : imageSmall.Image;

                var imageBig = dpcProduct.MarketingProduct.Images.FirstOrDefault(mp => mp.Type.Id == 2712);
                model.ImageBig = imageBig == null ? null : imageBig.Image;

                model.Order = dpcProduct.SortOrder;

                model.Benefit = parseType == ProductParseType.Mobapp ? HtmlParser.ToText(dpcProduct.MarketingProduct.Benefit) : dpcProduct.MarketingProduct.Benefit;
                model.Legal = HtmlParse(dpcProduct.MarketingProduct.Legal, dpcProduct.Id, parseType);
            }

            if (model.Benefit == null)
                model.Benefit = parseType == ProductParseType.Mobapp ? HtmlParser.ToText(dpcProduct.Benefit) : dpcProduct.Benefit;


            if (model.Legal == null)
                model.Legal = HtmlParse(dpcProduct.Legal, dpcProduct.Id, parseType);

            model.ParameterGroups = new List<ParameterGroup>();
            foreach (var parameterGroup in dpcProduct.Parameters
                .Where(p => p.Group != null)
                //            .Where(p => p.Group != null && p.Group.Id != 2203)
                .GroupBy(p => p.Group.Title))
            {
                var benefitGroup = new ParameterGroup() { Id = parameterGroup.First().Group.Id, Name = parameterGroup.Key, SortOrder = parameterGroup.First().Group.SortOrder ?? 0 };
                var parameters = parameterGroup.Where(pg => !pg.Modifiers.Any(m => m.Id == 2450));
                benefitGroup.ParameterArray = Mapper.Map<List<Parameter>>(parameters).OrderBy(p => p.SortOrder).ToList();


                if (parseType == ProductParseType.Mobapp)
                    Parallel.ForEach(benefitGroup.ParameterArray, (parameter) =>
                    {
                        parameter.Name = HtmlParser.ToText(parameter.Name);
                        parameter.StrValue = HtmlParser.ToText(parameter.StrValue);

                    });



                if (benefitGroup.ParameterArray.Any())
                    model.ParameterGroups.Add(benefitGroup);
            }
            model.ParameterGroups = model.ParameterGroups.OrderBy(pg => pg.SortOrder).ToList();


        }

        public object HtmlParse(string source, int id, ProductParseType parseType)
        {
            if (String.IsNullOrEmpty(source))
                return null;

            switch (parseType)
            {
                case ProductParseType.Mobapp:
                    return HtmlParser.ParseToJsonParts(source, id);
                case ProductParseType.Text:
                    return HtmlParser.ToText(source);
                case ProductParseType.Html:
                    return source;
                default:
                    return source;
            }

        }

        public IQueryable<MobileService> GetServicesByFilter(ProductFilter filter)
        {

            IQueryable<MobileService> mobileServices = Db.MobileServices.AsNoTracking();

            if (filter.isPublic != null)
            {
                mobileServices = mobileServices.Where(x => x.HideForUnauthorized == !filter.isPublic);
            }

            if (filter.isArchive != null)
            {
                mobileServices = mobileServices.Where(x => x.IsArchive == filter.isArchive);
            }
            if (filter.isB2B != null)
            {
                mobileServices = mobileServices.Where(x => x.IsB2B == filter.isB2B);
            }


            if (filter.isPrePaid != null)
            {
                mobileServices = mobileServices.Where(x => x.IsPrepaid == filter.isPrePaid);
            }
            if (!String.IsNullOrEmpty(filter.marketCode))
            {
                mobileServices = mobileServices.Where(x => x.Regions.Any(r => r.MarketCode.Equals(filter.marketCode.Trim(), StringComparison.CurrentCultureIgnoreCase)));
            }

            if (filter.isTraffic != null)
            {
                mobileServices = mobileServices.Where(x => x.IsHighway == filter.isTraffic);
            }

            if (filter.arrSoc != null)
            {
                mobileServices = mobileServices.Where(p => filter.arrSoc.Contains(p.SocName));



                //если один SOC, возвращаем одну услугу
                if ((filter.arrSoc.Count() == 1) && (mobileServices.Count() > 1))
                {
                    mobileServices = mobileServices.OrderBy(o => o.SortOrder).Take(1);
                    //ApiBackendEvents.Log.OneSocServicesException(filter);
                }

            }

            return mobileServices;
        }

        public IQueryable<MobileTariff> GetTariffsByFilter(ProductFilter filter)
        {
            IQueryable<MobileTariff> mobileTariffs = Db.MobileTariffs.AsNoTracking();

            if (filter.isPublic != null)
            {
                mobileTariffs = mobileTariffs.Where(x => x.HideForUnauthorized == !filter.isPublic);
            }

            if (filter.isArchive != null)
            {
                mobileTariffs = mobileTariffs.Where(x => x.IsArchive == filter.isArchive);
            }
            if (filter.isB2B != null)
            {
                mobileTariffs = mobileTariffs.Where(x => x.IsB2B == filter.isB2B);
            }


            if (filter.isPrePaid != null)
            {
                mobileTariffs = mobileTariffs.Where(x => x.IsPrepaid == filter.isPrePaid);
            }
            if (!String.IsNullOrEmpty(filter.marketCode))
            {
                mobileTariffs = mobileTariffs.Where(x => x.Regions.Any(r => r.MarketCode.Equals(filter.marketCode.Trim(), StringComparison.CurrentCultureIgnoreCase)));
            }
            if (filter.arrSoc != null)
            {
                mobileTariffs = mobileTariffs.Where(p => filter.arrSoc.Contains(p.SocName));
                //если один SOC, возвращаем одину тариф
                if ((filter.arrSoc.Count() == 1) && (mobileTariffs.Count() > 1))
                {
                    mobileTariffs = mobileTariffs.OrderBy(o => o.SortOrder).Take(1);
                    //ApiBackendEvents.Log.OneSocServicesException(filter);
                }
            }

            if (filter.isTraffic != null)
            {
                mobileTariffs = mobileTariffs.Where(x => x.HasMobileInternet == filter.isTraffic);
            }

            return mobileTariffs;
        }


        public IQueryable<MobileService> GetAllServices()
        {
            return Db.MobileServices;
        }

        public IQueryable<MobileTariff> GetAllTariffs()
        {
            return Db.MobileTariffs;
        }



        public IOrderedEnumerable<ServiceViewModelRMR> MapResInfoServiceRMR(List<MobileService> mobileServices, ProductFilter filter, ProductParseType parseType)
        {
            var rezInfosDictionary = new ConcurrentDictionary<Guid, ServiceViewModelRMR>();

            Parallel.ForEach(mobileServices, (service) =>
            {
                var info = MapService(service, filter.marketCode, parseType)
                        .Info
                        .SelectMany(s => MapServicesRMR(s, filter));

                if (info.Any())
                {
                    foreach (var inf in info.Where(i => i.services.Any()))
                    {
                        rezInfosDictionary.TryAdd(Guid.NewGuid(), inf);
                    }

                }
                else
                {
                    //ApiBackendEvents.Log.LogTraceInfo("Product was not mapped or equals NULL. Skippping product", service.ProductId);
                }
            });

            var result = rezInfosDictionary.Select(s => s.Value).GroupBy(g => g.id).Select(s =>
            {
                var serv = s.FirstOrDefault() ?? new ServiceViewModelRMR() { id = "service_section_general", visible = false };

                return new ServiceViewModelRMR()
                {
                    id = serv.id,
                    title = serv.title,
                    order = serv.order,
                    visible = serv.visible,
                    services = s.SelectMany(d => d.services).Distinct()
                                .GroupBy(gb => gb.id).Select(sv =>
                                {
                                    var svSoc = sv.FirstOrDefault() ?? new ServiceDO();
                                    return new ServiceDO()
                                    {
                                        id = svSoc.id,
                                        code = sv.SelectMany(m => m.code).ToList(),
                                        region = svSoc.region,
                                        ussd_on = svSoc.ussd_on,
                                        ussd_off = svSoc.ussd_off,
                                        name = svSoc.name,
                                        tagline = svSoc.tagline,
                                        is_promo = svSoc.is_promo,
                                        promo_text = svSoc.promo_text,
                                        promo_description = svSoc.promo_description,
                                        is_plan_opt = svSoc.is_plan_opt,
                                        promo_image = svSoc.promo_image,
                                        promo_image2 = svSoc.promo_image2,
                                        promo_order = svSoc.promo_order,
                                        visible = svSoc.visible,
                                        order = svSoc.order,
                                        section_id = svSoc.section_id,
                                        short_data = svSoc.short_data,
                                        description = svSoc.description,
                                        is_archive = svSoc.is_archive

                                    };
                                }).ToList()
                };
            }).OrderBy(o => o.order);
            return result;
        }


        public IOrderedEnumerable<TarifViewModelRMR> MapResInfoTariffRMR(List<MobileTariff> mobileTariffs, ProductFilter filter, ProductParseType parseType)
        {

            var rezInfosDictionary = new ConcurrentDictionary<Guid, TarifViewModelRMR>();

            Parallel.ForEach(mobileTariffs, (tariff) =>
            {
                //try
                //{
                var mappedInfo = MapTariff(tariff, filter.marketCode, parseType);
                var info = mappedInfo
                    .Info != null ? mappedInfo.Info.SelectMany(s => MapTariffsRMR(s, filter)) : null;

                if (info != null && info.Any())
                {

                    foreach (var inf in info)
                    {
                        rezInfosDictionary.TryAdd(Guid.NewGuid(), inf);
                    }

                }
                else
                {
                    //ApiBackendEvents.Log.LogTraceInfo("Product was not mapped or equals NULL. Skippping product", tariff.ProductId);
                }
                // }
                //catch (Exception e)
                //{
                //    ApiBackendEvents.Log.LogTraceInfo("tariff map error", tariff);
                //    ApiBackendEvents.Log.GeneralExceptionError(e);
                //}
            });

            //rezInfos = rezInfosDictionary.Select(s=>s.Value).ToList();

            var result = rezInfosDictionary.Select(s => s.Value).GroupBy(g => g.id).Select(s =>
            {
                var trf = s.FirstOrDefault() ?? new TarifViewModelRMR() { id = "plan_section_general", visible = false };

                return new TarifViewModelRMR()
                {
                    id = trf.id,
                    title = trf.title,
                    order = trf.order,
                    visible = trf.visible,
                    plans = s.SelectMany(d => d.plans).Distinct()
                                .GroupBy(gb => gb.id).Select(sv =>
                                {
                                    var svSoc = sv.FirstOrDefault() ?? new PlanDO();
                                    return new PlanDO()
                                    {
                                        id = svSoc.id,
                                        code = sv.SelectMany(m => m.code).ToList(),
                                        region = svSoc.region,
                                        ussd_on = svSoc.ussd_on,
                                        ussd_off = svSoc.ussd_off,
                                        name = svSoc.name,
                                        tagline = svSoc.tagline,
                                        is_promo = svSoc.is_promo,
                                        promo_text = svSoc.promo_text,
                                        promo_description = svSoc.promo_description,
                                        is_plan_opt = svSoc.is_plan_opt,
                                        promo_image = svSoc.promo_image,
                                        promo_image2 = svSoc.promo_image2,
                                        promo_order = svSoc.promo_order,
                                        visible = svSoc.visible,
                                        order = svSoc.order,
                                        section_id = svSoc.section_id,
                                        short_data = svSoc.short_data,
                                        description = svSoc.description,
                                        is_archive = svSoc.is_archive,
                                        weight = 0

                                    };
                                }).ToList()

                };
            }).OrderBy(o => o.order);
            return result;
        }


        public IEnumerable<ServiceViewModelRMR> MapServicesRMR(ServiceViewModel servicesInfo, ProductFilter filter)
        {
            var categories = servicesInfo.Categories;

            if (!categories.Any() && !(filter.isPublic ?? false))  //Не выводим, если нет категорий и запрашивают публичные
                yield return new ServiceViewModelRMR()
                {
                    id = "service_section_general",
                    visible = false,
                    services = new List<ServiceDO>() { MapServiceDO(servicesInfo, filter, "service_section_general") }
                };


            foreach (var category in categories)
            {
                //servicesInfo;
                var categoryId = "service_section_" + category.Id;
                yield return new ServiceViewModelRMR()
                {
                    id = categoryId,
                    title = category.Title,
                    order = category.SortOrder ?? 0,
                    visible = true,
                    services = new List<ServiceDO>() { MapServiceDO(servicesInfo, filter, categoryId) }
                };

            }
        }


        public IEnumerable<TarifViewModelRMR> MapTariffsRMR(TarifViewModel tariffInfo, ProductFilter filter)
        {
            var categories = tariffInfo.Categories;

            if (!categories.Any() && !(filter.isPublic ?? false))  //Не выводим, если нет категорий и запрашивают публичные
                yield return new TarifViewModelRMR()
                {
                    id = "plan_section_general",
                    visible = false,
                    plans = new List<PlanDO>() { MapPlanDO(tariffInfo, filter, "plan_section_general") }
                };


            foreach (var category in categories)
            {
                //servicesInfo;
                var categoryId = "plan_section_" + category.Id;
                yield return new TarifViewModelRMR()
                {
                    id = categoryId,
                    title = category.Title,
                    order = category.SortOrder ?? 0,
                    visible = true,
                    plans = new List<PlanDO>() { MapPlanDO(tariffInfo, filter, categoryId) }
                };

            }
        }

        public PlanDO MapPlanDO(TarifViewModel tariff, ProductFilter filter, string categoryId)
        {

            if (tariff.IconParameters.Any(p => p.Id == 0))
            {
                var tt = 0;
            }
            var tariffDescription = new List<DescriptionDO>();
            var order = 1;
            tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId, order, "large", false, new List<string> { tariff.Benefit, "" }));
            order++;
            tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_button", order, "button", false, new List<string> { "Перейти на этот тариф", "" }));

            var paymentParamGroups = tariff.ParameterGroups.Where(p => p.Id == 2177 || p.Id == 2364 || p.Id == 1950420);
            foreach (var paymentParam in paymentParamGroups.SelectMany(paymentParams => paymentParams.ParameterArray))
            {
                order++;
                tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_" + paymentParam.Id, order,
                    "subscriber-payment", false, new List<string> { paymentParam.Name, string.IsNullOrEmpty(paymentParam.StrValue) ? string.Format("{0:0.##}", paymentParam.Value) + " " + paymentParam.ValuePeriodMobile : paymentParam.StrValue }));
            }


            var cardParamGroups = tariff.ParameterGroups.Where(p => p.Id != 2177 && p.Id != 2364 && p.Id != 1950420 && p.Id != 1893393 && p.Id != 2203 && p.Id != 1776703);


            foreach (var cardParamGroup in cardParamGroups.Where(g => g.ParameterArray.Any(a => !a.HideInMobile)))
            {
                order++;
                tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_" + cardParamGroup.Id + "_separator", order, "separator", true, new List<string> { "", "" }));
                order++;
                tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_" + cardParamGroup.Id, order, "normal", false, new List<string> { cardParamGroup.Name, "" }));

                foreach (var cardParam in cardParamGroup.ParameterArray.Where(p => !p.HideInMobile))
                {
                    order++;
                    if (cardParam.Value == null && string.IsNullOrEmpty(cardParam.StrValue))
                        tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_" + cardParam.Id, order, "terms", false,
                            new List<string> { cardParam.Name, "" }));
                    else
                        tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_" + cardParam.Id, order, "price", false,
                            new List<string> { cardParam.Name, string.IsNullOrEmpty(cardParam.StrValue) ? string.Format("{0:0.##}", cardParam.Value) + " " + cardParam.ValuePeriodMobile : cardParam.StrValue }));
                }
            }

            order++;
            tariffDescription.Add(new DescriptionDO("plandesc_" + tariff.SiteProductId + "_more", order, "button-more", false,
                new List<string> { "Подробнее на www.beeline.ru", tariff.SiteUrl }));

            return new PlanDO
            {

                id = "plan_" + tariff.SiteProductId,
                code = new List<CodeDO>() { new CodeDO() { name = tariff.Soc } },
                region = filter.marketCode,

                ussd_on = tariff.ParameterGroups.Select(p =>
                {
                    var param = p.ParameterArray.Where(s => s.BaseParameter != null).FirstOrDefault(s => s.BaseParameter != null && (s.BaseParameter.Id == 2238 || s.BaseParameter.Id == 2234) && !string.IsNullOrEmpty(s.StrValue));
                    return param != null ? param.StrValue : null;
                }).FirstOrDefault(m => m != null), //USSS-код для подключения продукта

                ussd_off = tariff.ParameterGroups.Select(p =>
                {
                    var param = p.ParameterArray.FirstOrDefault(s => s.BaseParameter != null && (s.BaseParameter.Id == 2239 || s.BaseParameter.Id == 2235) && !string.IsNullOrEmpty(s.StrValue));
                    return param != null ? param.StrValue : null;
                }).FirstOrDefault(m => m != null),//USSS-код для отключения продукта

                name = tariff.Name,
                tagline = tariff.Benefit,
                is_promo = false,
                promo_text = tariff.Benefit,
                promo_description = "",
                is_plan_opt = false,
                promo_image = tariff.ImageSmall != null ? tariff.ImageSmall.Replace("https:", "").Replace("http:", "") : null,
                promo_image2 = tariff.ImageBig != null ? tariff.ImageBig.Replace("https:", "").Replace("http:", "") : null,
                visible = tariff.NoAuthVisible,
                order = tariff.Order,
                section_id = categoryId,
                is_archive = tariff.isArchive, //Признак архивного продукта
                short_data = tariff.IconParameters.Select(m => MapShortDataDO(m, "plandata")).OrderBy(o => o.order).ToList(), //Массив с параметрами для краткого описания продукта (иконки)
                description = tariffDescription, //Массив с параметрами для описания продукта в карточке
                weight = 0
            };
        }
        public ServiceDO MapServiceDO(ServiceViewModel service, ProductFilter filter, string categoryId)
        {

            var serviceDescription = new List<DescriptionDO>();
            var order = 1;
            serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId, order, "large", false, new List<string> { service.Benefit, "" }));
            order++;
            serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_button", order, "button", false, new List<string> { "Подключить", "" }));

            var paymentParamGroups = service.ParameterGroups.Where(p => p.Id == 2177 || p.Id == 2364 || p.Id == 1950420);
            foreach (var paymentParam in paymentParamGroups.SelectMany(paymentParams => paymentParams.ParameterArray))
            {
                order++;
                serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_" + paymentParam.Id, order,
                    "subscriber-payment", false, new List<string> { paymentParam.Name, string.IsNullOrEmpty(paymentParam.StrValue) ? string.Format("{0:0.##}", paymentParam.Value) + " " + paymentParam.ValuePeriodMobile : paymentParam.StrValue }));
            }


            var cardParamGroups = service.ParameterGroups.Where(p => p.Id != 2177 && p.Id != 2364 && p.Id != 1950420 && p.Id != 1893393 && p.Id != 2203 && p.Id != 1776703);


            foreach (var cardParamGroup in cardParamGroups.Where(g => g.ParameterArray.Any(a => !a.HideInMobile)))
            {
                order++;
                serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_" + cardParamGroup.Id + "_separator", order, "separator", true, new List<string> { "", "" }));
                order++;
                serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_" + cardParamGroup.Id, order, "normal", false, new List<string> { cardParamGroup.Name, "" }));

                foreach (var cardParam in cardParamGroup.ParameterArray.Where(p => !p.HideInMobile))
                {
                    order++;
                    if (cardParam.Value == null && string.IsNullOrEmpty(cardParam.StrValue))
                        serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_" + cardParam.Id, order, "terms", false,
                            new List<string> { cardParam.Name, "" }));
                    else
                        serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_" + cardParam.Id, order, "price", false,
                            new List<string> { cardParam.Name, string.IsNullOrEmpty(cardParam.StrValue) ? string.Format("{0:0.##}", cardParam.Value) + " " + cardParam.ValuePeriodMobile : cardParam.StrValue }));
                }
            }

            order++;
            serviceDescription.Add(new DescriptionDO("servicedesc_" + service.SiteProductId + "_more", order, "button-more", false,
                new List<string> { "Подробнее на www.beeline.ru", service.SiteUrl }));

            return new ServiceDO
            {

                id = "service_" + service.SiteProductId,
                code = new List<CodeDO> { new CodeDO { name = service.Soc } },
                region = filter.marketCode,

                ussd_on = service.ParameterGroups.Select(p =>
                {
                    var param = p.ParameterArray.Where(s => s.BaseParameter != null).FirstOrDefault(s => s.BaseParameter != null && (s.BaseParameter.Id == 2238 || s.BaseParameter.Id == 2234) && !string.IsNullOrEmpty(s.StrValue));
                    return param != null ? param.StrValue : null;
                }).FirstOrDefault(m => m != null), //USSS-код для подключения продукта

                ussd_off = service.ParameterGroups.Select(p =>
                {
                    var param = p.ParameterArray.FirstOrDefault(s => s.BaseParameter != null && (s.BaseParameter.Id == 2239 || s.BaseParameter.Id == 2235) && !string.IsNullOrEmpty(s.StrValue));
                    return param != null ? param.StrValue : null;
                }).FirstOrDefault(m => m != null),//USSS-код для отключения продукта

                name = service.Name,
                tagline = service.Benefit,
                is_promo = false,
                promo_text = service.Benefit,
                promo_description = "",
                is_plan_opt = false,
                promo_image = service.ImageSmall != null ? service.ImageSmall.Replace("https:", "").Replace("http:", "") : null,
                promo_image2 = service.ImageBig != null ? service.ImageBig.Replace("https:", "").Replace("http:", "") : null,
                visible = service.NoAuthVisible,
                order = service.Order,
                section_id = categoryId,
                is_archive = service.isArchive, //Признак архивного продукта
                short_data = service.IconParameters.Select(m => MapShortDataDO(m, "servicedata")).OrderBy(o => o.order).ToList(), //Массив с параметрами для краткого описания продукта (иконки)
                description = serviceDescription
                // description = service.Description!=null ?((List<DescriptionDO>)service.Description).Select(d => new DescriptionDO("servicedesc_" + service.SiteProductId, d.order, d.type, d.show_separator, d.strings)).ToList()
                //                                       : null
                //Массив с параметрами для описания продукта в карточке

            };
        }

        public ShortDataDO MapShortDataDO(IconParameter iconParameter, string prefix)
        {
            //if (iconParameter.Id == 0)
            //{
            //    var aa = 0;
            //}
            var shortDataDo = new ShortDataDO()
            {
                id = prefix + "_" + iconParameter.Id,
                price = string.Format("{0:0.##}", iconParameter.Value),
                units = iconParameter.ValuePeriodMobile
            };

            switch (iconParameter.TypeId)
            {
                case IconParameterType.Cost:
                    shortDataDo.order = 0;
                    shortDataDo.icon = "ch-period.png";
                    break;
                case IconParameterType.Communication:
                    shortDataDo.order = 1;
                    shortDataDo.icon = "ch-phone.png";
                    break;
                case IconParameterType.Messages:
                    shortDataDo.order = 2;
                    shortDataDo.icon = "ch-sms.png";
                    break;
                case IconParameterType.Internet:
                    shortDataDo.order = 3;
                    shortDataDo.icon = "ch-internet.png";
                    break;
                case IconParameterType.ServiceConnection:
                    shortDataDo.order = 4;
                    shortDataDo.icon = "ch-on.png";
                    break;
            }

            return shortDataDo;
        }
    }




    
    /// <summary>
    /// Parse Type
    /// </summary>
    public enum ProductParseType
    {
        [Description("Text")]
        Text = 1,
        [Description("HTML")]
        Html = 2,
        [Description("mobapp")]
        Mobapp = 3
    }

    public class ProductViewModel
    {
        public ProductViewModel()
        {
            Categories = new List<Category>();
            ParameterGroups = new List<ParameterGroup>();
            IconParameters = new List<IconParameter>();
            isArchive = false;
        }
        public int SiteProductId { get; set; }
        public int? Order { get; set; }
        public string Name { get; set; }
        [IgnoreDataMember]
        public string Soc { get; set; }
        public object Legal { get; set; }
        public string Benefit { get; set; }
        public List<Category> Categories { get; set; }
        public List<ParameterGroup> ParameterGroups { get; set; }
        [IgnoreDataMember]
        public List<IconParameter> IconParameters { get; set; }
        public bool NoAuthVisible { get; set; }
        public string ImageSmall { get; set; }
        public string ImageBig { get; set; }
        public string SiteUrl { get; set; }
        [IgnoreDataMember]
        public bool isArchive { get; set; }
    }

    public class ParameterGroup
    {
        public ParameterGroup()
        {
            ParameterArray = new List<Parameter>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Parameter> ParameterArray { get; set; }
        public int SortOrder { get; set; }
    }

    public class Parameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Value { get; set; }
        public string StrValue { get; set; }
        public string ValuePeriod { get; set; }
        public string ValuePeriodMobile { get; set; }
        [IgnoreDataMember]
        public string Type { get; set; }
        public bool HideInMobile { get; set; }
        public BaseParameter BaseParameter { get; set; }
        public int SortOrder { get; set; }
    }

    public class BaseParameter
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class TarifViewModel : ProductViewModel
    {
        //public List<Parameter> SummaryParameters { get; set; }
        [IgnoreDataMember]
        public List<string> RelevantServices { get; set; }
    }

    public class TarifInfo
    {
        public string Soc { get; set; }
        public IOrderedEnumerable<TarifViewModel> Info { get; set; }
    }

    public static class XmlDeserialize
    {
        public static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        static object XmlDeserializeFromString(string objectData, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }


    public enum IconParameterType
    {

        [Description("Абонентская плата")]
        Cost = 0,
        [Description("Пакет минут")]
        Communication = 1,
        [Description("Пакет SMS")]
        Messages = 2,
        [Description("Пакет интернета")]
        Internet = 3,
        [Description("Стоимость подключения (цена)")]
        ServiceConnection = 4
    }

    public class IconParameter
    {
        //[IgnoreDataMember]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Value { get; set; }
        public string StrValue { get; set; }
        public string ValuePeriod { get; set; }
        public string ValuePeriodMobile { get; set; }
        public string Type { get; set; }
        [IgnoreDataMember]
        public int SortOrder { get; set; }
        //[JsonProperty("Id")]
        [IgnoreDataMember]
        public IconParameterType TypeId { get; set; }

    }

    public class Category
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? SortOrder { get; set; }
    }


    public class ServicesInfo
    {
        public string Soc { get; set; }
        public IOrderedEnumerable<ServiceViewModel> Info { get; set; }
    }

    public class ServiceViewModel : ProductViewModel
    {
        public object Description { get; set; }
        [IgnoreDataMember]
        public List<string> RelevantTarifs { get; set; }
    }

    public static class HtmlParser
    {
        private static readonly string[] LargeElements = { "h1", "h2", "h3", "h4", "h5", "h6" };
        private static readonly string[] ButtonElements = { "button", "a" };
        private static readonly string[] ListElements = { "ul", "ol" };
        private static readonly string[] SeparatorElements = { "hr" };
        private static readonly string[] NormalElements = { "p", "#text" };
        private static readonly string[] RemoveElements = { "script", "#comment" };

        public static string ToText(string source)
        {
            return String.IsNullOrEmpty(source) ? source : HtmlCharsToUnicode(HtmlTagsRemove(source));
        }

        public static string HtmlTagsRemove(string source)
        {
            return String.IsNullOrEmpty(source) ? source : Regex.Replace(source, "<.*?>", string.Empty);
        }

        public static string HtmlCommentsRemove(string source)
        {
            return String.IsNullOrEmpty(source) ? source : Regex.Replace(source, "<!.*->", string.Empty, RegexOptions.Singleline);
        }

        public static string ScriptsRemove(string source)
        {
            return String.IsNullOrEmpty(source) ? source : Regex.Replace(source, "<script.*script>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        public static string HtmlCharsToUnicode(string source, MatchCollection badChars = null)
        {
            if (String.IsNullOrEmpty(source))
                return source;

            if (badChars == null)
                badChars = Regex.Matches(source, "&[a-zA-Z0-9]+;");

            var unicodeString = new StringBuilder(source);

            foreach (Match badChar in badChars)
                unicodeString.Replace(badChar.Value, WebUtility.HtmlDecode(badChar.Value));
            unicodeString.Replace("\n", "\r\n");

            return unicodeString.ToString();
        }

        public static List<DescriptionDO> ParseToJsonParts(string html, int id)
        {
            html = HtmlCommentsRemove(html);
            html = ScriptsRemove(html);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var jsonParts = new List<DescriptionDO>();

            var nodes = htmlDocument.DocumentNode.ChildNodes.ToArray();
            var order = 0;
            foreach (var node in nodes)
            {

                var badChars = Regex.Matches(node.InnerText, "&[a-zA-Z0-9]+;");

                foreach (Match badChar in badChars)
                {
                    //ApiBackendEvents.Log.HtmlParseCharException(id, badChar.Value);
                }


                var innerText = node.InnerText.Trim();
                if (string.IsNullOrEmpty(innerText) || RemoveElements.Contains(node.Name))
                {
                    continue;
                }

                order++;

                if (ListElements.Contains(node.Name))
                {
                    bool isYellowdotList = ListElements.Contains(node.Name) && (node.Attributes["class"] != null && node.Attributes["class"].Value.Split(' ').Contains("YELLOWDOT_LIST"));
                    jsonParts.AddRange(isYellowdotList ? CreateYellowdotListElementItem(node, order) : CreateListElementItem(node, order));
                    continue;
                }

                innerText = HtmlCharsToUnicode(innerText, badChars);

                if (SeparatorElements.Contains(node.Name))
                {
                    jsonParts.Add(CreateSeparatorElementItem(order));
                    continue;
                }

                if (LargeElements.Contains(node.Name))
                {
                    jsonParts.Add(CreateLargeElementItem(innerText, order));
                    continue;
                }

                if (ButtonElements.Contains(node.Name))
                {
                    jsonParts.Add(CreateButtonElementItem(innerText, order));
                    continue;
                }


                if (NormalElements.Contains(node.Name))
                {
                    jsonParts.Add(CreateNormalElementItem(innerText, order));
                    continue;
                }

                jsonParts.Add(CreateBadElementItem(node.Name, innerText, id, order));
            }

            return jsonParts;
        }

        #region JsonPartElements

        private static List<DescriptionDO> CreateYellowdotListElementItem(HtmlNode node, int order)
        {

            return node.SelectNodes("li").Select(m => new DescriptionDO(Guid.NewGuid().ToString(),
                order,
                "YELLOWDOT_LIST",
                false, new List<string> { HtmlCharsToUnicode(m.InnerText) })).ToList();

        }

        private static DescriptionDO CreateSeparatorElementItem(int order)
        {
            return new DescriptionDO(
                Guid.NewGuid().ToString(),
                order,
                "separator",
                true,
                null
                );
        }

        private static List<DescriptionDO> CreateListElementItem(HtmlNode node, int order)
        {
            return node.SelectNodes("li").Select((m, index) => new DescriptionDO(
               Guid.NewGuid().ToString(),
               order,
               "list",
               false,
               new List<string> { node.Name == "ul" ? "●" : (index + 1) + ".", HtmlCharsToUnicode(m.InnerText) })).ToList();
        }

        private static DescriptionDO CreateNormalElementItem(string innerText, int order)
        {

            return new DescriptionDO(
                Guid.NewGuid().ToString(),
                order,
                "normal",
                false,
                new List<string> { innerText, "" });
        }


        private static DescriptionDO CreateBadElementItem(string nodeName, string innerText, int id, int order)
        {

            //ApiBackendEvents.Log.HtmlParseTagException(id, nodeName);

            return CreateNormalElementItem(innerText, order);

        }


        private static DescriptionDO CreateButtonElementItem(string innerText, int order)
        {
            return new DescriptionDO(
                Guid.NewGuid().ToString(),
                order,
                "button",
                false,
                new List<string> { innerText });

        }

        private static DescriptionDO CreateLargeElementItem(string innerText, int order)
        {
            return new DescriptionDO(
               Guid.NewGuid().ToString(),
               order,
               "large",
               false,
               new List<string> { innerText, "" });
        }
        #endregion
    }

    public class DescriptionDO
    {
        public string id { get; set; }
        public int order { get; set; }
        public string type { get; set; }
        public bool show_separator { get; set; }
        public List<string> strings { get; set; }

        public DescriptionDO(string id, int order, string type, bool show_separator, List<string> strings)
        {
            this.id = id;
            this.order = order;
            this.type = type;
            this.show_separator = show_separator;
            this.strings = strings;
        }
    }





    public class ProductFilter
    {
        [Required]
        public string marketCode { get; set; }
        public string[] arrSoc { get; set; }
        public bool? isPrePaid { get; set; }
        public bool? isPublic { get; set; }
        public bool? isB2B { get; set; }
        public bool? isArchive { get; set; }
        public bool? isTraffic { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }


        public ProductFilter()
        {
            page = 1;
            pageSize = 10;
        }

        public ProductFilter(string marketCode, bool? isPrePaid = null, bool? isPublic = null, bool? isB2B = null,
            bool? isArchive = null, bool? isTraffic = null, int page = 1, int pageSize = 10)
        {
            this.marketCode = marketCode;
            this.isPrePaid = isPrePaid;
            this.isPublic = isPublic;
            this.isB2B = isB2B;
            this.isArchive = isArchive;
            this.isTraffic = isTraffic;
            this.page = page;
            this.pageSize = pageSize;
        }
    }



    public class ServiceViewModelRMR
    {
        public string id { get; set; } //Id группы продуктов
        public int? order { get; set; } //Порядок вывода группы продуктов в списке
        public string title { get; set; } //Заголовок группы
        public bool visible { get; set; } //Признак отображения группы
        public List<ServiceDO> services { get; set; }
    }

    public class ServiceDO
    {
        public string id { get; set; }
        public List<CodeDO> code { get; set; } //Массив соков услуг, соответствующих продукту
        public string region { get; set; } //Маркет-код региона продукта
        public string ussd_on { get; set; } //USSS-код для подключения продукта
        public string ussd_off { get; set; } //USSS-код для отключения продукта
        public string name { get; set; } //Название продукта
        public string tagline { get; set; } //Краткое описание услуги (в карточке услуги после картинки)
        public bool is_promo { get; set; } //Признак рекомендуемого продукта
        public string promo_text { get; set; } //Краткое описание услуги в разделе Рекомендуемые
        public string promo_description { get; set; } //Краткое описание рекомендуемой услуги в карточке
        public bool is_plan_opt { get; set; } //Признак того, что услуга является опцией тарифного плана
        public string promo_image { get; set; } //Ссылка на картинку для списка
        public string promo_image2 { get; set; } //Ссылка на картинку для карточки услуги
        public int promo_order { get; set; } //Порядок вывода продукта в списке рекомендуемых Чем меньше значение, тем выше порядок отображения
        public bool visible { get; set; } //Признак отображения продукта
        public int? order { get; set; } //Порядок вывода продукта в списке Чем меньше значение, тем выше порядок отображения
        public string section_id { get; set; } //Идентификатор группы, в которую входит продукт
        public List<ShortDataDO> short_data { get; set; } //Массив с параметрами для краткого описания продукта (иконки)
        public List<DescriptionDO> description { get; set; } //Массив с параметрами для описания продукта в карточке
        public bool is_archive { get; set; } //Признак архивного продукта
    }


    public class CodeDO
    {
        public string name { get; set; } //Сок услуги, соответствующей продукту
    }


    public class ShortDataDO
    {
        public string id { get; set; }
        public int order { get; set; }
        public string icon { get; set; } //Название файла с иконкой (файл хранится на устройстве)
        public string price { get; set; }
        public string units { get; set; } //Единица изменения
    }




    public class TarifViewModelRMR
    {
        public string id { get; set; } //Id группы продуктов
        public int? order { get; set; } //Порядок вывода группы продуктов в списке
        public string title { get; set; } //Заголовок группы
        public bool visible { get; set; } //Признак отображения группы
        public List<PlanDO> plans { get; set; }
    }

    public class PlanDO
    {
        public string id { get; set; }
        public List<CodeDO> code { get; set; } //Массив соков услуг, соответствующих продукту
        public string region { get; set; } //Маркет-код региона продукта
        public string ussd_on { get; set; } //USSS-код для подключения продукта
        public string ussd_off { get; set; } //USSS-код для отключения продукта
        public string name { get; set; } //Название продукта
        public string tagline { get; set; } //Краткое описание услуги (в карточке услуги после картинки)
        public bool is_promo { get; set; } //Признак рекомендуемого продукта
        public string promo_text { get; set; } //Краткое описание услуги в разделе Рекомендуемые
        public string promo_description { get; set; } //Краткое описание рекомендуемой услуги в карточке
        public bool is_plan_opt { get; set; } //Признак того, что услуга является опцией тарифного плана
        public string promo_image { get; set; } //Ссылка на картинку для списка
        public string promo_image2 { get; set; } //Ссылка на картинку для карточки услуги
        public int promo_order { get; set; } //Порядок вывода продукта в списке рекомендуемых Чем меньше значение, тем выше порядок отображения
        public bool visible { get; set; } //Признак отображения продукта
        public int? order { get; set; } //Порядок вывода продукта в списке Чем меньше значение, тем выше порядок отображения
        public string section_id { get; set; } //Идентификатор группы, в которую входит продукт
        public List<ShortDataDO> short_data { get; set; } //Массив с параметрами для краткого описания продукта (иконки)
        public List<DescriptionDO> description { get; set; } //Массив с параметрами для описания продукта в карточке
        public bool is_archive { get; set; } //Признак архивного продукта
        public int weight { get; set; } //Вес ТП для раздела Рекомендуемые

        public PlanDO()
        {
            weight = 0;
        }
    }








}

