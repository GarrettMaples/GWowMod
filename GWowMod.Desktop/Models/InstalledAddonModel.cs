using System;
using AutoMapper;
using GWowMod.JSON;

namespace GWowMod.Desktop.Models
{
    public class InstalledAddonModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime FileDate { get; set; }
        public string DownloadUrl { get; set; }
        public bool HasUpdate { get; set; }
        public string UpdateFileName { get; set; }
        public DateTime? UpdateFileDate { get; set; }
        public string UpdateDownloadUrl { get; set; }
    }

    internal class InstalledAddonModelMapping : Profile
    {
        public InstalledAddonModelMapping()
        {
            CreateMap<ExactMatch, InstalledAddonModel>()
                .ForMember
                (
                    dest => dest.Id,
                    opt => opt.MapFrom(x => x.Id)
                )
                .ForMember
                (
                    dest => dest.FileName,
                    opt => opt.MapFrom(x => x.File.FileName)
                )
                .ForMember
                (
                    dest => dest.FileDate,
                    opt => opt.MapFrom(x => x.File.FileDate)
                )
                .ForMember
                (
                    dest => dest.DownloadUrl,
                    opt => opt.MapFrom(x => x.File.DownloadUrl)
                )
                .ForMember
                (
                    dest => dest.HasUpdate,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id)
                )
                .ForMember
                (
                    dest => dest.UpdateFileName,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id ? x.LatestFile.FileName : string.Empty)
                )
                .ForMember
                (
                    dest => dest.UpdateFileDate,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id ? x.LatestFile.FileDate : (DateTime?) null)
                )
                .ForMember
                (
                    dest => dest.UpdateDownloadUrl,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id ? x.LatestFile.DownloadUrl : string.Empty)
                );
        }
    }
}
