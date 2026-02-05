using AutoMapper;
using B2BProcurement.Business.DTOs.Company;
using B2BProcurement.Business.DTOs.Material;
using B2BProcurement.Business.DTOs.Quotation;
using B2BProcurement.Business.DTOs.Rfq;
using B2BProcurement.Business.DTOs.User;
using B2BProcurement.Core.Entities;

namespace B2BProcurement.Business.Mappings
{
    /// <summary>
    /// AutoMapper profili.
    /// Entity-DTO dönüşümlerini tanımlar.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Company Mappings
            CreateCompanyMappings();

            // Material Mappings
            CreateMaterialMappings();

            // RFQ Mappings
            CreateRfqMappings();

            // Quotation Mappings
            CreateQuotationMappings();

            // User Mappings
            CreateUserMappings();

            // Supplier Mappings
            CreateSupplierMappings();

            // Notification Mappings
            CreateNotificationMappings();
        }

        #region Company Mappings

        private void CreateCompanyMappings()
        {
            // Entity → Dto
            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null))
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : null));

            // Entity → ListDto
            CreateMap<Company, CompanyListDto>()
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null))
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : null));

            // CreateDto → Entity
            CreateMap<CompanyCreateDto, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Package, opt => opt.Ignore())
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.Materials, opt => opt.Ignore())
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.RFQs, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore());

            // UpdateDto → Entity
            CreateMap<CompanyUpdateDto, Company>()
                .ForMember(dest => dest.TaxNumber, opt => opt.Ignore()) // TaxNumber güncellenemez
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Package, opt => opt.Ignore())
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.Materials, opt => opt.Ignore())
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.RFQs, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore());
        }

        #endregion

        #region Material Mappings

        private void CreateMaterialMappings()
        {
            // Entity → Dto
            CreateMap<Material, MaterialDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null));

            // Entity → ListDto
            CreateMap<Material, MaterialListDto>()
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null));

            // CreateDto → Entity
            CreateMap<MaterialCreateDto, Material>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore()) // Service'de set edilecek
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.RFQItems, opt => opt.Ignore());

            // UpdateDto → Entity
            CreateMap<MaterialUpdateDto, Material>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.RFQItems, opt => opt.Ignore());
        }

        #endregion

        #region RFQ Mappings

        private void CreateRfqMappings()
        {
            // Entity → Dto
            CreateMap<RFQ, RfqDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
                .ForMember(dest => dest.QuotationCount, opt => opt.MapFrom(src => src.Quotations.Count));

            // Entity → ListDto
            CreateMap<RFQ, RfqListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
                .ForMember(dest => dest.QuotationCount, opt => opt.MapFrom(src => src.Quotations.Count));

            // Entity → DetailDto (kalemler, dokümanlar ve kişiler dahil)
            CreateMap<RFQ, RfqDetailDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts));

            // RFQItem → RfqItemDto
            CreateMap<RFQItem, RfqItemDto>()
                .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : null));

            // RFQContact → RfqContactDto
            CreateMap<RFQContact, RfqContactDto>();

            // CreateDto → Entity
            CreateMap<RfqCreateDto, RFQ>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RfqNumber, opt => opt.Ignore()) // Otomatik oluşturulacak
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore()) // Service'de set edilecek
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore()) // Ayrı işlenecek
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore());

            // RfqItemCreateDto → RFQItem
            CreateMap<RfqItemCreateDto, RFQItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RfqId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.RFQ, opt => opt.Ignore())
                .ForMember(dest => dest.Material, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationItems, opt => opt.Ignore());

            // RfqContactCreateDto → RFQContact
            CreateMap<RfqContactCreateDto, RFQContact>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RfqId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.RFQ, opt => opt.Ignore());

            // UpdateDto → Entity
            CreateMap<RfqUpdateDto, RFQ>()
                .ForMember(dest => dest.RfqNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore());
        }

        #endregion

        #region Quotation Mappings

        private void CreateQuotationMappings()
        {
            // Entity → Dto
            CreateMap<Quotation, QuotationDto>()
                .ForMember(dest => dest.RfqNumber, opt => opt.MapFrom(src => src.RFQ != null ? src.RFQ.RfqNumber : null))
                .ForMember(dest => dest.RfqTitle, opt => opt.MapFrom(src => src.RFQ != null ? src.RFQ.Title : null))
                .ForMember(dest => dest.SupplierCompanyName, opt => opt.MapFrom(src => src.SupplierCompany != null ? src.SupplierCompany.CompanyName : null))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

            // Entity → ListDto
            CreateMap<Quotation, QuotationListDto>()
                .ForMember(dest => dest.RfqNumber, opt => opt.MapFrom(src => src.RFQ != null ? src.RFQ.RfqNumber : null))
                .ForMember(dest => dest.SupplierCompanyName, opt => opt.MapFrom(src => src.SupplierCompany != null ? src.SupplierCompany.CompanyName : null))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

            // QuotationItem → QuotationItemDto
            CreateMap<QuotationItem, QuotationItemDto>()
                .ForMember(dest => dest.RfqItemDescription, opt => opt.MapFrom(src => src.RFQItem != null ? src.RFQItem.Description : null))
                .ForMember(dest => dest.RequestedQuantity, opt => opt.MapFrom(src => src.RFQItem != null ? src.RFQItem.Quantity : 0))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.RFQItem != null ? src.RFQItem.Unit : null));

            // CreateDto → Entity
            CreateMap<QuotationCreateDto, Quotation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationNumber, opt => opt.Ignore()) // Otomatik oluşturulacak
                .ForMember(dest => dest.SupplierCompanyId, opt => opt.Ignore()) // Service'de set edilecek
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.RFQ, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierCompany, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore()) // Ayrı işlenecek
                .ForMember(dest => dest.Documents, opt => opt.Ignore());

            // QuotationItemCreateDto → QuotationItem
            CreateMap<QuotationItemCreateDto, QuotationItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // Hesaplanacak
                .ForMember(dest => dest.ApprovalStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Quotation, opt => opt.Ignore())
                .ForMember(dest => dest.RFQItem, opt => opt.Ignore());
        }

        #endregion

        #region User Mappings

        private void CreateUserMappings()
        {
            // Entity → Dto
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null));

            // CreateDto → Entity
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Service'de hash'lenecek
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore());

            // RegisterDto → User
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Service'de hash'lenecek
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // CompanyAdmin olarak set edilecek
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore()) // Yeni şirket oluşturulacak
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore());

            // RegisterDto → Company (Kayıt sırasında şirket oluşturma için)
            CreateMap<RegisterDto, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TaxOffice, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Phone, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PackageId, opt => opt.Ignore()) // Varsayılan paket atanacak
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore())
                .ForMember(dest => dest.Package, opt => opt.Ignore())
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.Materials, opt => opt.Ignore())
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.RFQs, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore());
        }

        #endregion

        #region Supplier Mappings

        private void CreateSupplierMappings()
        {
            // Entity → Dto
            CreateMap<Supplier, DTOs.Supplier.SupplierDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null));

            // Entity → ListDto
            CreateMap<Supplier, DTOs.Supplier.SupplierListDto>()
                .ForMember(dest => dest.SectorName, opt => opt.MapFrom(src => src.Sector != null ? src.Sector.Name : null));

            // CreateDto → Entity
            CreateMap<DTOs.Supplier.SupplierCreateDto, Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore());

            // UpdateDto → Entity
            CreateMap<DTOs.Supplier.SupplierUpdateDto, Supplier>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Sector, opt => opt.Ignore());
        }

        #endregion

        #region Notification Mappings

        private void CreateNotificationMappings()
        {
            // Entity → Dto
            CreateMap<Notification, DTOs.Notification.NotificationDto>();

            // CreateDto → Entity
            CreateMap<DTOs.Notification.NotificationCreateDto, Notification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsRead, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }

        #endregion
    }
}
