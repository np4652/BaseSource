using BaseSource.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseSource.AppCode.Service
{
    public class DbContext : IDbContext
    {
        private readonly IDapper _dapper;
        public DbContext(IDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<List<BBPSOutlet>> BBPSOutletDetail(int top)
        {
            List<BBPSOutlet> result = new List<BBPSOutlet>();
            try
            {
                var sqlQuery = @$"select Top {top} _Id id,_AgentName AgentName,_ShopName ShopName,_Mobile Mobile,_Address [Address],
                                         _PinCode PinCode,_City City,_State [State] 
                                 from tbl_NearestBBPOutlet(nolock) where ISNULL(_IsSent,0) = 0 order by _id";
                result = await _dapper.GetAll<BBPSOutlet>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
            }
            catch (Exception ex)
            {
                await LogError(ex.Message, "DbContext-->BBPSOutletDetail");
            }
            return result;
        }

        public async Task<List<BBPSOutlet>> BBPSOutletDetail()
        {
            List<BBPSOutlet> result = new List<BBPSOutlet>();
            try
            {
                var sqlQuery = @"IF(Select count(1) from tbl_NearestBBPOutlet(nolock) where ISNULL(_IsSent,0) = 0) = 0 
                                    update tbl_NearestBBPOutlet set _IsSent = 0
                                 select Top 1000 _Id id,_AgentName AgentName,_ShopName ShopName,_Mobile Mobile,_Address [Address],
                                                 _PinCode PinCode,_City City,_State [State] 
                                 from tbl_NearestBBPOutlet(nolock) where ISNULL(_IsSent,0) = 0 order by _id
                                 update top (1000) tbl_NearestBBPOutlet set _IsSent = 1 where ISNULL(_IsSent,0) = 0";
                result = await _dapper.GetAll<BBPSOutlet>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
            }
            catch (Exception ex)
            {
                await LogError(ex.Message, "DbContext-->BBPSOutletDetail");
            }
            return result;
        }

        public async Task<List<BBPSOutlet>> BBPOutletMobiles()
        {
            List<BBPSOutlet> result = new List<BBPSOutlet>();
            try
            {
                var sqlQuery = @"IF (Select count(1) from tbl_BBPOutletMobiles(nolock) where ISNULL(_IsSent,0) = 0) = 0 
                                    update tbl_BBPOutletMobiles set _IsSent = 0
                                 select Top 1000 _Id id,_Mobile Mobile from tbl_BBPOutletMobiles(nolock) where ISNULL(_IsSent,0) = 0 order by _Id
                                 update top (1000) tbl_BBPOutletMobiles set _IsSent = 1 where ISNULL(_IsSent,0) = 0";
                result = await _dapper.GetAll<BBPSOutlet>(sqlQuery, new DynamicParameters(), System.Data.CommandType.Text);
            }
            catch (Exception ex)
            {
                await LogError(ex.Message, "DbContext-->BBPOutletMobiles");
            }
            return result;
        }
        public async Task LogError(string error, string origin)
        {
            try
            {
                var sqlQuery = @"insert into tbl_PageErrorLog(_ClsName,_FnName,_UserID,_Error,_EntryDate,_LoginTypeID) values('waba.360dialog',@origin,1,@error,GETDATE(),1)";
                var dbparams = new DynamicParameters();
                dbparams.Add("origin", origin);
                dbparams.Add("error", error);
                await _dapper.Update<int>(sqlQuery, dbparams, System.Data.CommandType.Text);
            }
            catch (Exception ex)
            {
                await LogError(ex.Message, "DbContext-->LogError");
            }
        }

        public async Task createLog(string response, string action = "", string type = "")
        {
            var sqlQuery = @"insert into log_Wab360dialog_VerifyResponse(_response,_EntryOn,_Action,_type) values(@response,Getdate(),@action,@type)";
            _ = await _dapper.Update<int>(sqlQuery, new { response, action, type }, System.Data.CommandType.Text);
        }
    }
}
