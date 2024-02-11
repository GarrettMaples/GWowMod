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
        public string LatestFileName { get; set; }
        public DateTime? LatestFileDate { get; set; }
        public string LatestDownloadUrl { get; set; }
    }

    internal class InstalledAddonModelMapping : Profile
    {
        public InstalledAddonModelMapping()
        {
            CreateMap<Match, InstalledAddonModel>()
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
                    dest => dest.LatestFileName,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id ? x.LatestFile.FileName : string.Empty)
                )
                .ForMember
                (
                    dest => dest.LatestFileDate,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id ? x.LatestFile.FileDate : (DateTime?) null)
                )
                .ForMember
                (
                    dest => dest.LatestDownloadUrl,
                    opt => opt.MapFrom(x => x.LatestFile != null && x.LatestFile.Id != x.File.Id ? x.LatestFile.DownloadUrl : string.Empty)
                );
        }
    }
}
