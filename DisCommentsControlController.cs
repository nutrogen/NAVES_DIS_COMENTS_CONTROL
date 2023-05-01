using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using NuGet.Protocol.Core.Types;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using NavesPortalforWebWithCoreMvc.Common;
using NavesPortalforWebWithCoreMvc.RfSystemModels;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Syncfusion.EJ2.Base;
using System.Collections;

namespace NavesPortalforWebWithCoreMvc.Controllers.DIS
{
    [Authorize]
    [CheckSession]
    public class DisCommentsControlController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _systemContext;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public DisCommentsControlController(BM_NAVES_PortalContext repository, RfSystemContext systemContext, IBM_NAVES_PortalContextProcedures procedures)
        {
            _repository = repository;
            _systemContext = systemContext;
            _procedures = procedures;
        }

        /// <summary>
        /// Project Modnitoring
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View());
        }

        /// <summary>
        /// 비동기 데이터 소스
        /// </summary>
        /// <param name="SearchString"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="dm"></param>
        /// <returns></returns>
        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null || SearchString == String.Empty)
                {
                    SearchString = "";
                }

                List<PNAV_DIS_GET_WORK_LIST_IN_PROJECTResult> resultList = await _procedures.PNAV_DIS_GET_WORK_LIST_IN_PROJECTAsync(SearchString, StartDate, EndDate);

                IEnumerable DataSource = resultList.AsEnumerable();
                DataOperations operation = new DataOperations();

                //Search
                if (dm.Search != null && dm.Search.Count > 0)
                {
                    DataSource = operation.PerformSearching(DataSource, dm.Search);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
                {
                    DataSource = operation.PerformSorting(DataSource, dm.Sorted);
                }

                //Filtering
                if (dm.Where != null && dm.Where.Count > 0)
                {
                    DataSource = operation.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
                }

                int count = DataSource.Cast<PNAV_DIS_GET_WORK_LIST_IN_PROJECTResult>().Count();

                //Paging
                if (dm.Skip != 0)
                {

                    DataSource = operation.PerformSkip(DataSource, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    DataSource = operation.PerformTake(DataSource, dm.Take);
                }

                return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(new { result = DataSource });
            }
            catch (Exception e)
            {
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "DisProjectMonitoring", returnView = "Index" });
            }
        }

        /// <summary>
        /// WORI_ID
        /// </summary>
        /// <param name="id">WORI_ID</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Work ID
            ViewBag.dataSource = await _repository.VNAV_SELECT_DIS_ITP_WORK_LISTs.Where(m => m.WORK_IDX == id).ToListAsync();

            // Comment List
            List<VNAV_SELECT_DIS_ITP_CONTROL_LIST> itpLIst = await _repository.VNAV_SELECT_DIS_ITP_CONTROL_LISTs.Where(m => m.WORK_IDX == id).ToListAsync();
            ViewBag.dataSourceItpList = itpLIst;
            ViewBag.WorkIdx = id;
            ViewBag.count = itpLIst.Count();

            return View();
        }

        /// <summary>
        /// Work ID 별 ITP List
        /// </summary>
        /// <param name="WorkID"></param>
        /// <param name="ItpStatus"></param>
        /// <param name="ActionStatus"></param>
        /// <param name="SearchString"></param>
        /// <param name="dm"></param>
        /// <returns></returns>
        public async Task<IActionResult> UrlDataSourceItpList(Guid Work_Idx, string? SearchString, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null || SearchString == String.Empty)
                {
                    SearchString = "";
                }

                List<PNAV_DIS_GET_COMMENT_LIST_FROM_WORKResult> resultList = await _procedures.PNAV_DIS_GET_COMMENT_LIST_FROM_WORKAsync(Work_Idx, SearchString);

                IEnumerable DataSource = resultList.AsEnumerable();
                DataOperations operation = new DataOperations();

                //Search
                if (dm.Search != null && dm.Search.Count > 0)
                {
                    DataSource = operation.PerformSearching(DataSource, dm.Search);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
                {
                    DataSource = operation.PerformSorting(DataSource, dm.Sorted);
                }

                //Filtering
                if (dm.Where != null && dm.Where.Count > 0)
                {
                    DataSource = operation.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
                }

                int count = DataSource.Cast<PNAV_DIS_GET_COMMENT_LIST_FROM_WORKResult>().Count();

                //Paging
                if (dm.Skip != 0)
                {

                    DataSource = operation.PerformSkip(DataSource, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    DataSource = operation.PerformTake(DataSource, dm.Take);
                }

                return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(new { result = DataSource });
            }
            catch (Exception e)
            {
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "DisItpControl", returnView = "Index" });
            }
        }

        /// <summary>
        /// POPUP 상세
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CommentCheckPopup(Guid WorkIdx, Guid CommentIdx)
        {
            List<PNAV_DIS_GET_COMMENT_DETAILResult> comment = await _procedures.PNAV_DIS_GET_COMMENT_DETAILAsync(WorkIdx, CommentIdx);
            return await Task.Run(()=> View(comment.First()));
        }
    }
}
